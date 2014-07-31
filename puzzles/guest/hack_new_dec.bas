5        REM  +------------------------------------------------+
10       REM  | HACK.BAS      (c) 19100   fr33 v4r14bl3z       |
15       REM  |                                                |
20       REM  | Brute-forces passwords on UM vIX.0 systems.    |
25       REM  | Compile with Qvickbasic VII.0 or later:        |
30       REM  |    /bin/qbasic hack.bas                        |
35       REM  | Then run:                                      |
40       REM  |   ./hack.exe username                          |
45       REM  |                                                |
50       REM  | This program is for educational purposes only! |
55       REM  +------------------------------------------------+
60       REM
65       IF ARGS() > 1 THEN GOTO 85
70       PRINT "usage: ./hack.exe username"
75       PRINT CHR(10)
80       END
85       REM
90       REM  get username from command line
95       DIM username AS STRING
100      username = ARG(2)
105      REM  common words used in passwords
110      DIM pwdcount AS INTEGER
115      pwdcount = 53
120      DIM words(pwdcount) AS STRING
125      words(1) = "airplane"
130      words(2) = "alphabet"
135      words(3) = "aviator"
140      words(4) = "bidirectional"
145      words(5) = "changeme"
150      words(6) = "creosote"
155      words(7) = "cyclone"
160      words(8) = "december"
165      words(9) = "dolphin"
170      words(10) = "elephant"
175      words(11) = "ersatz"
180      words(12) = "falderal"
185      words(13) = "functional"
190      words(14) = "future"
195      words(15) = "guitar"
200      words(16) = "gymnast"
205      words(17) = "hello"
210      words(18) = "imbroglio"
215      words(19) = "january"
220      words(20) = "joshua"
225      words(21) = "kernel"
230      words(22) = "kingfish"
235      words(23) = "(\b.bb)(\v.vv)"
240      words(24) = "millennium"
245      words(25) = "monday"
250      words(26) = "nemesis"
255      words(27) = "oatmeal"
260      words(28) = "october"
265      words(29) = "paladin"
270      words(30) = "pass"
275      words(31) = "password"
280      words(32) = "penguin"
285      words(33) = "polynomial"
290      words(34) = "popcorn"
295      words(35) = "qwerty"
300      words(36) = "sailor"
305      words(37) = "swordfish"
310      words(38) = "symmetry"
315      words(39) = "system"
320      words(40) = "tattoo"
325      words(41) = "thursday"
330      words(42) = "tinman"
335      words(43) = "topography"
340      words(44) = "unicorn"
345      words(45) = "vader"
350      words(46) = "vampire"
355      words(47) = "viper"
360      words(48) = "warez"
365      words(49) = "xanadu"
370      words(50) = "xyzzy"
375      words(51) = "zephyr"
380      words(52) = "zeppelin"
385      words(53) = "zxcvbnm"
390      REM try each password
395      PRINT "attempting hack with " + pwdcount + " passwords " + CHR(10)
400      DIM i AS INTEGER
405      i = 1
410      IF CHECKPASS(username, words(i)) THEN GOTO 430
415      i = i + 1
420      IF i > pwdcount THEN GOTO 445
425      GOTO 410
430      PRINT "found match!! for user " + username + CHR(10)
435      PRINT "password: " + words(i) + CHR(10)
440      END
445      PRINT "no simple matches for user " + username + CHR(10)
450      REM
455      REM  the above code will probably crack passwords for many
460      REM  users so I always try it first. when it fails, I try the
465      REM  more expensive method below.
470      REM
475      REM  passwords often take the form
480      REM    dictwordDD
485      REM  where DD is a two-digit decimal number. try these next:
490      i = 1
495      DIM n AS INTEGER
500      DIM m AS INTEGER
505      i = i + 1
510      IF i > pwdcount+1 THEN GOTO 560
515      n = 47
520      n = n + 1
525      IF n > 57 THEN GOTO 505
530      m = 48
535      REM PRINT "check " + words(i-1) + CHR(n) + CHR(m) + "..." + CHR(10) 
540      IF CHECKPASS(username, words(i-1) + CHR(n) + CHR(m)) THEN GOTO 570
545      m = m + 1
550      IF m > 57 THEN GOTO 520
555      GOTO 535
560      PRINT "no smart mathes for user " + username + CHR(10)
565      END
570      PRINT "found match! for user " + username + CHR(10)
575      PRINT "password: " + words(i-1) + CHR(n) + CHR(m) + CHR(10)
580      END
