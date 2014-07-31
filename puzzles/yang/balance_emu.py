#!/usr/bin/python

from array import array
import sys

modulo32 = 2**32
modulo8 = 2**8

CODE = array('b')
M = array('b', [0]*256)
IP = 0
IS = 1
sR = array('b', [0]*4)
dR = array('b', [0]*2)

while True:
  instr = CODE[IP]

  opcode = instr >> 5
  IMM =instr & 0b00011111
  D = (instr & 0b00010000) >> 4
  S1 = (instr & 0b00001100) >> 2
  S2 = instr & 0b00000011

  # MATH
  if opcode == 0b001:         
    M[dR[(D+1)%2]] = (M[sR[(S1+1)%4]] - M[sR[(S2+1)%4]]) % modulo8
    M[dR[D]] = M[sR[S1]] + M[sR[S2]]
  
  # LOGIC
  elif opcode == 0b010:
    M[dR[(D+1)%2]] = M[sR[(S1+1)%4]] ^ M[sR[(S2+1)%4]]
    M[dR[D]] = M[sR[S1]] & M[sR[S2]]

  # SCIENCE
  elif opcode == 0b000:
    if M[sR[0]] != 0:
      IS = IMM
    if IS == 0:
      print "Machine halt."
      sys.exit(0)

  # PHYSICS
  elif opcode == 0b011:
    minplus = (IMM & 0b00010000) >> 4
    val = IMM & 0b00001111
    if minplus == 1: # minus
      sR[0] = sR[0] - val  
    else: # plus
      sR[0] = sR[0] + val
    
    

  # OTHER CODE
  else:
    print "BAIL. Machine terminate."
    sys.exit(1)

  IP = (((IP + IS) % modulo32) % len(CODE))


