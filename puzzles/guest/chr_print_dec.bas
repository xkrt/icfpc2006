5     DIM i AS INTEGER
10    i = 20
15    PRINT i + ":" + CHR(i) + CHR(10)
20    i = i + 1
25    IF i > 100 THEN GOTO 35
30    GOTO 15
35    PRINT "END"
