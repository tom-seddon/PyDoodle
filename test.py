#import pydoodle
from pydoodle import *
import math
import testmodule
import sys
#import argparse

print sys.path
print sys.modules

class Test:
    def __init__(self):
        print "Fred.__init__: self=%d"%id(self)
        print "testmodule.func()=%s"%testmodule.func()
        print "testmodule.func2()=%s"%testmodule.func2()
        self.start=V2(0,0)
        self.end=V2(10,10)
        self.str="Test String"
        tweak(self,"start","end","str")

    def tick(self):
        line(self.start,self.end)
        circle(V2(0,0),10)

run(Test())
