MKDIR:=mkdir.exe
CP:=cp.exe
SRC:=PyDoodle/bin/Release
DEST:=bin

.PHONY:default
default:
	$(error Specify target)

.PHONY:bin
bin:
	$(MAKE) .install FILE=PyDoodle.exe
	$(MAKE) .install FILE=PyDoodle.pdb
	$(MAKE) .install FILE=WeifenLuo.WinFormsUI.Docking.dll
	$(MAKE) .install FILE=WeifenLuo.WinformsUI.Docking.pdb
	$(MAKE) .install FILE=IronPython.*

.PHONY:.install
.install:
	$(MKDIR) -p $(DEST)
	$(CP) $(SRC)/$(FILE) $(DEST)/
