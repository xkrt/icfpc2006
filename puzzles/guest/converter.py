#!/usr/bin/python
# convert roman->arabic, arabic->roman
# 19:09 25.07.2009
# written for Python 3.1

# -----------------------------------------------------------------------------
def decToRoman(num):
  coding = zip(
    [1000,900,500,400,100,90,50,40,10,9,5,4,1],
    ["M","CM","D","CD","C","XC","L","XL","X","IX","V","IV","I"]
  )
  result = []
  for d, r in coding:
    while num >= d:
      result.append(r)
      num -= d
  return ''.join(result)


# -----------------------------------------------------------------------------
def romToDec(str):
  result = 0
  lastValue = 0
  i = len(str)-1
  while i >= 0:
    v = str[i].upper()
    if v == 'C':
      curValue = 100
    elif v == 'D':
      curValue = 500
    elif v == 'I':
      curValue = 1
    elif v == 'L':
      curValue = 50
    elif v == 'M':
      curValue = 1000
    elif v == 'V':
      curValue = 5
    elif v == 'X':
      curValue = 10
    else:
      raise Exception('Invalid character: ' % v)
    if curValue < lastValue:
      result = result - curValue
    else:
      result = result + curValue
    lastValue = curValue
    i = i - 1
  return result


import re
import sys

# -----------------------------------------------------------------------------
def convDec2Rom(txt):
  def replFunc(mobj):
    num = decToRoman(int(mobj.group(0)))
#    print(mobj.group(0) + ':' + num)
    return num

  for line in txt:
    r1 = re.match('^([0-9]+)+\s+(.+)$', line)
    if r1:
      linenum = decToRoman(int(r1.group(1)))
      if not re.match('^REM\s+.+$', r1.group(2)):
        sub_ret = re.sub('[0-9]+', replFunc, r1.group(2))
        print('{0:9}{1}'.format(linenum, sub_ret))
      else:
        print('{0:9}{1}'.format(linenum, r1.group(2)))

# -----------------------------------------------------------------------------
def convRom2Dec(txt):
  def replFunc(mobj):
#    print(mobj.group(0) + ':' + str(romToDec(mobj.group(0))))
    return str(romToDec(mobj.group(0)))

  for line in txt:
    r1 = re.match('^([IVXLCDM]+)\s+(.+)$', line)
    if r1:
      linenum = romToDec(r1.group(1))
      if not re.match('^REM\s+.+$', r1.group(2)):
        sub_ret = re.sub('(?<= |\()[IVXLCDM]+(?= |$|\))', replFunc, r1.group(2))
        print('{0:9}{1}'.format(str(linenum), sub_ret))
      else:
        print('{0:9}{1}'.format(str(linenum), r1.group(2)))

# -----------------------------------------------------------------------------
if __name__ == '__main__':
  if len(sys.argv) != 3:
    print('usage: converter.py -d|-r hack.bas')
    print('       -d -- decimal to roman')
    print('       -r -- roman to decimal')
    sys.exit(0)

  txt = open(sys.argv[2]).readlines()
  if sys.argv[1] == '-d':
    convDec2Rom(txt)
  elif sys.argv[1] == '-r':
    convRom2Dec(txt)
  else:
    print('Unknown key')
    sys.exit(1)

