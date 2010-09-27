
def ParseOptions():
	AddOption('--opt',
		dest='opt',
		action='store_true',
		default='false',
		help='build in opt mode, or dbg mode')

ParseOptions()

env = DefaultEnvironment()

env.Append(LIBS=[
	'gflags',
	'glog',
	'gtest',
])

if GetOption('opt') == True:
	env['root'] = 'opt'
	env.Append(CCFLAGS='-g')
	env.Append(LIBS='tcmalloc_debug')
else:
	env['root'] = 'dbg'
	env.Append(CCFLAGS='-O2 -g')
	env.Append(LIBS='tcmalloc')

env.Append(CPPPATH=Dir(env['root']).abspath)

Export('env')

SConscript('src/SConscript', build_dir=env['root'])

