
def ParseOptions():
	AddOption('--mode',
		dest='mode',
		type='string',
		nargs=1,
		action='store',
		default='dbg',
		help='build in dbg or opt mode')

ParseOptions()

env = DefaultEnvironment()

env['mode'] = GetOption('mode')

env.Append(CPPPATH=Dir(env['mode']).abspath)

env.Append(LIBS=[
	'gflags',
	'glog',
	'gtest',
])

if env['mode'] == 'dbg':
	env.Append(CCFLAGS='-g')
	env.Append(LIBS='tcmalloc_debug')
else:
	env.Append(CCFLAGS='-O2 -g')
	env.Append(LIBS='tcmalloc')

Export('env')

SConscript('src/SConscript', build_dir=env['mode'])

