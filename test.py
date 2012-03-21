
#import pydoodle
from pydoodle import *
import math
import testmodule
import sys
import clr
#import argparse

print sys.path
print sys.modules

class Test:
	def __init__(self):
		print "Fred.__init__: self=%d"%id(self)
		print "testmodule.func()=%s"%testmodule.func()
		print "testmodule.func2()=%s"%testmodule.func2()
		self.start=V2(0,0)
		#self.end=V2(10,10)
		self.str="Test String"
		self.angle=0.0
		self.r=0.0
		tweaks(attrs(self,
			   "start",
			   #"end",
			   "str",
			   "angle",
			   "r"))

		self.theta=0

	def tick(self):
		delta=V2(math.sin(self.angle),math.cos(self.angle))
		line(self.start,self.start+delta*self.r)

		s=max(math.sin(self.theta),0)
		for i in range(10):
			circle(self.start,i*100+s*50)

		self.theta+=math.pi/8

# f=1.0
# v=V2(2,3)
# ref_test(clr.Reference(f),clr.Reference(v2))
# print "f=%s v=%s"%(f,v)
		
run(Test())

# Local Variables:
# indent-tabs-mode: 't
# tab-width: 4
# End:
