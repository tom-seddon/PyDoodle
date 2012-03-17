import pydoodle
from pydoodle import V2
import math

class Test:
    def __init__(self):
        print "Fred.__init__: self=%d"%id(self)

    def tick(self):
        pydoodle.line(V2(0,0),V2(10,10))

pydoodle.run(Test())

