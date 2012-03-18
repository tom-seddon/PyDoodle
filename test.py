#import pydoodle
from pydoodle import *
import math

class Test:
    def __init__(self):
        print "Fred.__init__: self=%d"%id(self)
        self.start=V2(0,0)
        self.end=V2(10,10)
        self.str="Test String"
        tweak(self,"start","end","str")

    def tick(self):
        line(self.start,self.end)
        circle(V2(0,0),10)

run(Test())
