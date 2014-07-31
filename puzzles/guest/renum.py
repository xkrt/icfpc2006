import sys
import re

with open(sys.argv[1]) as fd:
  i = 5
  for line in fd:
    m = re.match('^\d+\s+(.+)$', line)
    data = line
    if m:
      data = m.group(1)
    print('{0:9}{1}'.format(str(i), data))
    i = i + 5
