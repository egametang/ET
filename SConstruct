env = DefaultEnvironment()

def ParseOptions():
	AddOption('--opt',
		dest='opt',
		action='store_true',
		default='false',
		help='build in opt mode, default dbg mode')

ParseOptions()

env.Append(LIBPATH=[
	'/usr/local/lib'
])

env.Append(LIBS=[
	'gflags',
	'glog',
	'gtest',
	'gmock',
])

if GetOption('opt') == True:
	env['root'] = 'opt'
	env.Append(CCFLAGS='-O2 -g')
	env.Append(LIBS='tcmalloc')
else:
	env['root'] = 'dbg'
	env.Append(CCFLAGS='-g')
	env.Append(LIBS='tcmalloc_debug')

env.Append(CPPPATH=Dir(env['root']).abspath)


Export('env')

SConscript('src/SConscript', variant_dir=env['root'])

