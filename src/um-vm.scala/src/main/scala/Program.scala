import java.io.RandomAccessFile
import scala.annotation.tailrec
import scala.collection.mutable.ArrayBuffer

object Program {
  type Platter = Int
  type RegisterNum = Int
  type OrthographyValue = Int

  case class VmState(platters: ArrayBuffer[Array[Platter]],
                     pointer: Int,
                     reg: Array[Platter])

  abstract class Operation
  case class ConditionMove(a: RegisterNum, b: RegisterNum, c: RegisterNum) extends Operation
  case class ArrayIndex(a: RegisterNum, b: RegisterNum, c: RegisterNum) extends Operation
  case class ArrayAmendment(a: RegisterNum, b: RegisterNum, c: RegisterNum) extends Operation
  case class Addition(a: RegisterNum, b: RegisterNum, c: RegisterNum) extends Operation
  case class Multiplication(a: RegisterNum, b: RegisterNum, c: RegisterNum) extends Operation
  case class Division(a: RegisterNum, b: RegisterNum, c: RegisterNum) extends Operation
  case class NotAnd(a: RegisterNum, b: RegisterNum, c: RegisterNum) extends Operation
  case class Halt() extends Operation
  case class Allocation(b: RegisterNum, c: RegisterNum) extends Operation
  case class Abandonment(c: RegisterNum) extends Operation
  case class Output(c: RegisterNum) extends Operation
  case class Input(c: RegisterNum) extends Operation
  case class LoadProgram(b: RegisterNum, c: RegisterNum) extends Operation
  case class Orthography(a: RegisterNum, value: OrthographyValue) extends Operation

  def main(args: Array[String]) {
    args match {
      case Array(umPath) => {
        val platters = readScroll(umPath)
        val vmState = VmState(
          platters = ArrayBuffer(platters),
          pointer = 0,
          reg = Array.fill[Platter](8)(0))
        runVM(vmState)
      }
      case _ => printUsage
    }
  }

  def printUsage: Unit = {
    println("Universal Machine interpretator (Scala version) fro ICFPC 2006")
    println("Pavel Martynov aka xkrt, 2014-08-13")
    println("usage: java -jar um-vm.scala.jar <scroll file path>")
  }

  def readScroll (path: String): Array[Platter] = {
    val raf = new RandomAccessFile(path, "r")
    val scroll = new Array[Int]((raf.length() / 4).toInt)
    for (ind <- 0 to scroll.length-1)
      scroll(ind) = raf.readInt()
    scroll
  }

  def op (platter: Platter) = platter >>> 28
  def a (platter: Platter): RegisterNum = (platter >> 6) & 7
  def b (platter: Platter): RegisterNum = (platter >> 3) & 7
  def c (platter: Platter): RegisterNum = (platter >> 0) & 7
  def a_orto (platter: Platter): RegisterNum = (platter >> 25) & 7
  def value (platter: Platter) = platter & 0x1FFFFFF

  def toOperation(platter: Platter): Operation = {
    val opNum = op(platter)
    (opNum, platter) match {
      case (13, p) => Orthography(a = a_orto(p), value = value(p))
      case (1, p)  => ArrayIndex(a = a(p), b = b(p), c = c(p))
      case (2, p)  => ArrayAmendment(a = a(p), b = b(p), c = c(p))
      case (12, p) => LoadProgram(b = b(p), c = c(p))
      case (6, p)  => NotAnd(a = a(p), b = b(p), c = c(p))
      case (0, p)  => ConditionMove(a = a(p), b = b(p), c = c(p))
      case (3, p)  => Addition(a = a(p), b = b(p), c = c(p))
      case (8, p)  => Allocation(b = b(p), c = c(p))
      case (9, p)  => Abandonment(c = c(p))
      case (4, p)  => Multiplication(a = a(p), b = b(p), c = c(p))
      case (5, p)  => Division(a = a(p), b = b(p), c = c(p))
      case (10, p) => Output(c = c(p))
      case (11, p) => Input(c = c(p))
      case (7, p)  => Halt()
      case (n, _)  => sys.error(s"Unknown operation $n")
    }
  }

  @tailrec
  def runVM(vm: VmState): Unit = {
    val platter = vm.platters(0)(vm.pointer)
    val operation = toOperation(platter)
    var newPointer = vm.pointer + 1

    operation match {
      case ConditionMove(a,b,c) => if (vm.reg(c) != 0) vm.reg(a) = vm.reg(b)
      case ArrayIndex(a,b,c) => vm.reg(a) = vm.platters(vm.reg(b))(vm.reg(c))
      case ArrayAmendment(a,b,c) => vm.platters(vm.reg(a))(vm.reg(b)) = vm.reg(c)
      case Addition(a,b,c) => vm.reg(a) = vm.reg(b) + vm.reg(c)
      case Multiplication(a,b,c) => vm.reg(a) = vm.reg(b) * vm.reg(c)
      case Division(a,b,c) => vm.reg(a) = ((vm.reg(b) & 0xFFFFFFFFL) / (vm.reg(c) & 0xFFFFFFFFL)).toInt
      case NotAnd(a,b,c) => vm.reg(a) = ~(vm.reg(b) & vm.reg(c))
      case Halt() => println("\nHalt"); sys.exit(0)
      case Allocation(b,c) =>
        val newArray = Array.fill[Platter](vm.reg(c))(0)
        vm.platters.append(newArray)
        vm.reg(b) = vm.platters.length - 1
      case Abandonment(c) => vm.platters(vm.reg(c)) = null
      case Output(c) => System.out.print(vm.reg(c).toChar)
      case Input(c) => vm.reg(c) = System.in.read()
      case LoadProgram(b,c) =>
        if (vm.reg(b) != 0) {
          val sourceArray = vm.platters(vm.reg(b))
          vm.platters(0) = sourceArray.clone()
        }
        newPointer = vm.reg(c)
      case Orthography(a,value) => vm.reg(a) = value
    }
    runVM(vm.copy(pointer = newPointer))
  }
}
