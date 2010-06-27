env = DefaultEnvironment()

def ParseOptions():
	AddOption('--mode',
		dest='mode',
		type='string',
		nargs=1,
		action='store',
		default='dbg',
		help='build in dbg or opt mode')
	env['mode'] = GetOption('mode')

ParseOptions()

Export('env')

SConscript('src/SConscript' % x, build_dir=target_dir)

