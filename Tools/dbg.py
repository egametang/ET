#!/usr/bin/python
#-*- coding: utf-8 -*-

import sys, os

SAMBA_PATH = r"Z:/source/hainan"
LINUX_PATH = "/home/tanghai/source/hainan"

def main(argv):
	os.chdir(LINUX_PATH)
	cmd = "gdb " + ' '.join(argv[1:])
	cmd = cmd.replace("\\", "/")
	cmd = cmd.replace(SAMBA_PATH, ".")
	cmd = cmd.replace("\r\n", "\n")
	cmd = r'''sed -u -e 's/\\/\//g' | sed -u -e 's/%s/\./g' | ''' % \
		SAMBA_PATH.replace("/", r"\/\/") + cmd
	os.system(cmd)

if __name__ == '__main__':
	sys.exit(main(sys.argv))