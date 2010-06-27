
def ParseOptions():
	AddOption('--mode',
		dest='mode',
		type='string',
		nargs=1,
		action='store',
		default='dbg',
		help='build in dbg or opt mode')

env = DefaultEnvironment()

ParseOptions()
env['mode'] = GetOption('mode')

if env['mode'] == 'dbg':
	env.Append(CCFLAGS='-g')
else:
	env.Append(CCFLAGS='-O2 -g')

Export('env')

SConscript('src/SConscript', build_dir=env['mode'])

