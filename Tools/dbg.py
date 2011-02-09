#!/usr/bin/python
#-*- coding: utf-8 -*-

import sys, os

def main(argv):
  os.chdir("~/source/hainan")
  cmd = "gdb " + ' '.join(argv[1:])
  cmd = cmd.replace("\\", "/")
  cmd = cmd.replace(r"Z:/source/hainan", ".")
  cmd = cmd.replace("\r\n", "\n")
  cmd = r'''sed -u -e 's/\\/\//g' | sed -u -e 's/Z:\/\/source\/\/hainan/\./g' | ''' + cmd
  os.system(cmd)

if __name__ == '__main__':
  sys.exit(main(sys.argv))
