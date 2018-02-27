
# Compression

The `DeflateTest` program is mean to illustrate what kind of bandwith we need.

~~~
           Write: 139,287 op/s  32,358 KiB/s  4.30 op/KiB
            Read:  85,337 op/s  19,834 KiB/s  4.30 op/KiB
    DeflateWrite:  24,941 op/s   4,231 KiB/s  5.89 op/KiB
     DeflateRead:  38,180 op/s   6,525 KiB/s  5.85 op/KiB

1.36
~~~

Note that we can write a lot faster than we can read but as soon as we enable compression, writing takes a tremendous hit. However, without compression the bandwith requirement is 250 Mbit/s.