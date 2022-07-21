package {{namespace}}

import "bright/net"

type ProtocolFactory = func () net.Protocol

var ProtocolStub map[int]ProtocolFactory

func init() {
	ProtocolStub = make(map[int]ProtocolFactory)
	{{~for p in protos~}}
	ProtocolStub[{{p.id}}] = func () net.Protocol { return &{{p.go_full_name}}{} }
	{{~end~}}
	{{~for r in rpcs~}}
	ProtocolStub[{{r.id}}] = func () net.Protocol { return &{{r.go_full_name}}{} }
	{{~end~}}
}
