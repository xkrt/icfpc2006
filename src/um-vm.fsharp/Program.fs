module Program
open System
open System.IO

type Platter = uint32
type RegisterNum = int
type OrthographyValue = uint32

type Operation =
   | ConditionMove of RegisterNum * RegisterNum * RegisterNum
   | ArrayIndex of RegisterNum * RegisterNum * RegisterNum
   | ArrayAmendment of RegisterNum * RegisterNum * RegisterNum
   | Addition of RegisterNum * RegisterNum * RegisterNum
   | Multiplication of RegisterNum * RegisterNum * RegisterNum
   | Division of RegisterNum * RegisterNum * RegisterNum
   | NotAnd of RegisterNum * RegisterNum * RegisterNum
   | Halt
   | Allocation of RegisterNum * RegisterNum
   | Abandonment of RegisterNum
   | Output of RegisterNum
   | Input of RegisterNum
   | LoadProgram of RegisterNum * RegisterNum
   | Orthography of RegisterNum * OrthographyValue

let op platter : uint32 = platter >>> 28
let a platter = int (platter &&& 0x1c0u >>> 6)
let b platter = int (platter &&& 0x038u >>> 3)
let c platter = int (platter &&& 0x007u)
let a_orto platter = int (platter &&& 0xe000000u >>> 25)
let value platter = platter &&& 0x1ffffffu

type VM = {
    platters   : Platter array ResizeArray;
    pointer    : uint32;
    registers  : Platter array;
}

let breakToFourBytes (bytes : byte array) =
    let rec loop (result : byte array list) (left : byte list) =
        match left with
        | b1::b2::b3::b4::bs -> loop ([|b1;b2;b3;b4|] :: result) bs
        | _ -> result
    loop [] (Array.toList bytes) |> List.rev

let bytesToPlatter (bytes : byte array) : Platter =
    let revBytes = bytes |> Array.rev
    System.BitConverter.ToUInt32(revBytes, 0)

let readScroll scrollFilePath =
    File.ReadAllBytes scrollFilePath
    |> breakToFourBytes
    |> List.map bytesToPlatter

let constructVm (platters : Platter list) =
    let vm = {
        registers = Array.zeroCreate 8
        pointer = 0u
        platters = ResizeArray()
    }
    vm.platters.Add(List.toArray platters)
    vm

let (|IsBasicOperator|_|) (baicOpNum : uint32) (platOperatorNum, plat) =
    if platOperatorNum = baicOpNum
    then Some (a plat, b plat, c plat)
    else None

let (|IsHalt|_|) (opNum, _) = if opNum = 7u then Some() else None
let (|IsAllocation|_|) (opNum, plat) = if opNum = 8u then Some(b plat, c plat) else None
let (|IsAbandonment|_|) (opNum, plat) = if opNum = 9u then Some(c plat) else None
let (|IsOutput|_|) (opNum, plat) = if opNum = 10u then Some(c plat) else None
let (|IsInput|_|) (opNum, plat) = if opNum = 11u then Some(c plat) else None
let (|IsLoadProgram|_|) (opNum, plat) = if opNum = 12u then Some(b plat, c plat) else None
let (|IsOrthography|_|) (opNum, plat) = if opNum = 13u then Some(a_orto plat, value plat) else None

let platterToOperation (platter : Platter) =
    let operationNum = op platter
    match (operationNum, platter) with
    | IsOrthography (a,value) -> Orthography (a,value)
    | IsBasicOperator 1u (a,b,c) -> ArrayIndex (a,b,c)
    | IsBasicOperator 2u (a,b,c) -> ArrayAmendment (a,b,c)
    | IsLoadProgram (b,c) -> LoadProgram (b,c)
    | IsBasicOperator 6u (a,b,c) -> NotAnd (a,b,c)
    | IsBasicOperator 0u (a,b,c) -> ConditionMove (a,b,c)
    | IsBasicOperator 3u (a,b,c) -> Addition (a,b,c)
    | IsAllocation (b,c) -> Allocation (b,c)
    | IsAbandonment c -> Abandonment c
    | IsBasicOperator 4u (a,b,c) -> Multiplication (a,b,c)
    | IsBasicOperator 5u (a,b,c) -> Division (a,b,c)
    | IsOutput c -> Output c
    | IsInput c -> Input c
    | IsHalt -> Halt
    | _ -> failwithf "Unknown operation %d" operationNum

let stdoutStream = Console.OpenStandardOutput()

let rec runVm (vm : VM) =
    let platter = vm.platters.[0].[Convert.ToInt32(vm.pointer)]
    let operation = platterToOperation platter
    let mutable newPointer = vm.pointer + 1u

    match operation with
    | ConditionMove (a,b,c) -> if not (vm.registers.[c] = 0u)
                               then vm.registers.[a] <- vm.registers.[b]
    | ArrayIndex (a,b,c) -> vm.registers.[a] <- vm.platters.[int(vm.registers.[b])].[int(vm.registers.[c])]
    | ArrayAmendment (a,b,c) -> vm.platters.[int(vm.registers.[a])].[int(vm.registers.[b])] <- vm.registers.[c]
    | Addition (a,b,c) -> vm.registers.[a] <- vm.registers.[b] + vm.registers.[c]
    | Multiplication (a,b,c) -> vm.registers.[a] <- vm.registers.[b] * vm.registers.[c]
    | Division (a,b,c) -> vm.registers.[a] <- vm.registers.[b] / vm.registers.[c]
    | NotAnd (a,b,c) -> vm.registers.[a] <- ~~~(vm.registers.[b] &&& vm.registers.[c])
    | Halt -> printf "Halt"; exit 0
    | Allocation (b,c) ->
        let newArray = Array.zeroCreate(int(vm.registers.[c]))
        vm.platters.Add(newArray)
        vm.registers.[b] <- uint32(vm.platters.Count-1)
    | Abandonment c -> vm.platters.[int(vm.registers.[c])] <- null
    | Output c -> stdoutStream.WriteByte(byte vm.registers.[c])
    | Input c ->
        let key = Console.Read();
        if key = -1
        then vm.registers.[c] <- ~~~0u
        else vm.registers.[c] <- uint32 key
    | LoadProgram (b,c) ->
        let sourceArrayNum = int(vm.registers.[b])
        if not (sourceArrayNum = 0)
        then
            let sourceArray = vm.platters.[sourceArrayNum]
            let newArray = Array.zeroCreate(sourceArray.Length)
            Array.Copy(sourceArray, newArray, sourceArray.Length)
            vm.platters.[0] <- newArray
        newPointer <- vm.registers.[c]
    | Orthography (a,value) -> vm.registers.[a] <- value
    runVm { vm with pointer = newPointer }

let printUsage() =
    printfn "Universal Machine interpretator (F# version) for ICFPC 2006"
    printfn "Pavel Martynov aka xkrt, 2013-06-22"
    printfn "usage: um-vm.fsharp <scroll file path>"
    ()

[<EntryPoint>]
let main argv =
    match argv.Length with
    | 1 ->
        argv.[0]
        |> readScroll
        |> constructVm
        |> runVm
        exit 0
    | _ -> printUsage(); exit 1
