import testmodule2

def func():
    return "testmodule func"

def func2():
    return testmodule2.func()

if __name__=="__main__":
    print "executed."
    
