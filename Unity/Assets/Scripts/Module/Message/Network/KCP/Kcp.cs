using System;

public class Kcp
{
	public const int IKCP_RTO_NDL = 30; // no delay min rto
	public const int IKCP_RTO_MIN = 100; // normal min rto
	public const int IKCP_RTO_DEF = 200;
	public const int IKCP_RTO_MAX = 60000;
	public const int IKCP_CMD_PUSH = 81; // cmd: push data
	public const int IKCP_CMD_ACK = 82; // cmd: ack
	public const int IKCP_CMD_WASK = 83; // cmd: window probe (ask)
	public const int IKCP_CMD_WINS = 84; // cmd: window size (tell)
	public const int IKCP_ASK_SEND = 1; // need to send IKCP_CMD_WASK
	public const int IKCP_ASK_TELL = 2; // need to send IKCP_CMD_WINS
	public const int IKCP_WND_SND = 32;
	public const int IKCP_WND_RCV = 32;
	public const int IKCP_MTU_DEF = 1024;
	public const int IKCP_ACK_FAST = 3;
	public const int IKCP_INTERVAL = 100;
	public const int IKCP_OVERHEAD = 24;
	public const int IKCP_DEADLINK = 10;
	public const int IKCP_THRESH_INIT = 2;
	public const int IKCP_THRESH_MIN = 2;
	public const int IKCP_PROBE_INIT = 7000; // 7 secs to probe window size
	public const int IKCP_PROBE_LIMIT = 120000; // up to 120 secs to probe window

	// encode 8 bits unsigned int
	public static int ikcp_encode8u(byte[] p, int offset, byte c)
	{
		p[0 + offset] = c;
		return 1;
	}

	// decode 8 bits unsigned int
	public static int ikcp_decode8u(byte[] p, int offset, ref byte c)
	{
		c = p[0 + offset];
		return 1;
	}

	/* encode 16 bits unsigned int (lsb) */
	public static int ikcp_encode16u(byte[] p, int offset, UInt16 w)
	{
		p[0 + offset] = (byte)(w >> 0);
		p[1 + offset] = (byte)(w >> 8);
		return 2;
	}

	/* decode 16 bits unsigned int (lsb) */
	public static int ikcp_decode16u(byte[] p, int offset, ref UInt16 c)
	{
		UInt16 result = 0;
		result |= p[0 + offset];
		result |= (UInt16)(p[1 + offset] << 8);
		c = result;
		return 2;
	}

	/* encode 32 bits unsigned int (lsb) */
	public static int ikcp_encode32u(byte[] p, int offset, UInt32 l)
	{
		p[0 + offset] = (byte)(l >> 0);
		p[1 + offset] = (byte)(l >> 8);
		p[2 + offset] = (byte)(l >> 16);
		p[3 + offset] = (byte)(l >> 24);
		return 4;
	}

	/* decode 32 bits unsigned int (lsb) */
	public static int ikcp_decode32u(byte[] p, int offset, ref UInt32 c)
	{
		UInt32 result = 0;
		result |= p[0 + offset];
		result |= (UInt32)(p[1 + offset] << 8);
		result |= (UInt32)(p[2 + offset] << 16);
		result |= (UInt32)(p[3 + offset] << 24);
		c = result;
		return 4;
	}

	public static byte[] slice(byte[] p, int start, int stop)
	{
		var bytes = new byte[stop - start];
		Array.Copy(p, start, bytes, 0, bytes.Length);
		return bytes;
	}

	public static T[] slice<T>(T[] p, int start, int stop)
	{
		var arr = new T[stop - start];
		int index = 0;
		for (int i = start; i < stop; i++)
		{
			arr[index] = p[i];
			index++;
		}

		return arr;
	}

	public static byte[] append(byte[] p, byte c)
	{
		var bytes = new byte[p.Length + 1];
		Array.Copy(p, bytes, p.Length);
		bytes[p.Length] = c;
		return bytes;
	}

	public static T[] append<T>(T[] p, T c)
	{
		var arr = new T[p.Length + 1];
		for (int i = 0; i < p.Length; i++)
		{
			arr[i] = p[i];
		}

		arr[p.Length] = c;
		return arr;
	}

	public static T[] append<T>(T[] p, T[] cs)
	{
		var arr = new T[p.Length + cs.Length];
		for (int i = 0; i < p.Length; i++)
		{
			arr[i] = p[i];
		}

		for (int i = 0; i < cs.Length; i++)
		{
			arr[p.Length + i] = cs[i];
		}

		return arr;
	}

	private static UInt32 _imin_(UInt32 a, UInt32 b)
	{
		return a <= b ? a : b;
	}

	private static UInt32 _imax_(UInt32 a, UInt32 b)
	{
		return a >= b ? a : b;
	}

	private static UInt32 _ibound_(UInt32 lower, UInt32 middle, UInt32 upper)
	{
		return _imin_(_imax_(lower, middle), upper);
	}

	private static Int32 _itimediff(UInt32 later, UInt32 earlier)
	{
		return (Int32)(later - earlier);
	}

	// KCP Segment Definition
	internal class Segment
	{
		internal UInt32 conv;
		internal UInt32 cmd;
		internal UInt32 frg;
		internal UInt32 wnd;
		internal UInt32 ts;
		internal UInt32 sn;
		internal UInt32 una;
		internal UInt32 resendts;
		internal UInt32 rto;
		internal UInt32 fastack;
		internal UInt32 xmit;
		internal byte[] data;

		internal Segment(int size)
		{
			this.data = new byte[size];
		}

		// encode a segment into buffer
		internal int encode(byte[] ptr, int offset)
		{
			int offset_ = offset;

			offset += ikcp_encode32u(ptr, offset, conv);
			offset += ikcp_encode8u(ptr, offset, (byte)cmd);
			offset += ikcp_encode8u(ptr, offset, (byte)frg);
			offset += ikcp_encode16u(ptr, offset, (UInt16)wnd);
			offset += ikcp_encode32u(ptr, offset, ts);
			offset += ikcp_encode32u(ptr, offset, sn);
			offset += ikcp_encode32u(ptr, offset, una);
			offset += ikcp_encode32u(ptr, offset, (UInt32)data.Length);

			return offset - offset_;
		}
	}

	// kcp members.
	private readonly UInt32 conv;

	private UInt32 mtu;
	private UInt32 mss;
	private UInt32 state;
	private UInt32 snd_una;
	private UInt32 snd_nxt;
	private UInt32 rcv_nxt;
	private UInt32 ts_recent;
	private UInt32 ts_lastack;
	private UInt32 ssthresh;
	private UInt32 rx_rttval;
	private UInt32 rx_srtt;
	private UInt32 rx_rto;
	private UInt32 rx_minrto;
	private UInt32 snd_wnd;
	private UInt32 rcv_wnd;
	private UInt32 rmt_wnd;
	private UInt32 cwnd;
	private UInt32 probe;
	private UInt32 current;
	private UInt32 interval;
	private UInt32 ts_flush;
	private UInt32 xmit;
	private UInt32 nodelay;
	private UInt32 updated;
	private UInt32 ts_probe;
	private UInt32 probe_wait;
	private readonly UInt32 dead_link;
	private UInt32 incr;

	private Segment[] snd_queue = new Segment[0];
	private Segment[] rcv_queue = new Segment[0];
	private Segment[] snd_buf = new Segment[0];
	private Segment[] rcv_buf = new Segment[0];

	private UInt32[] acklist = new UInt32[0];

	private byte[] buffer;
	private Int32 fastresend;
	private Int32 nocwnd;

	private Int32 logmask;

	// buffer, size
	private readonly Action<byte[], int> output;

	// create a new kcp control object, 'conv' must equal in two endpoint
	// from the same connection.
	public Kcp(UInt32 conv_, Action<byte[], int> output_)
	{
		conv = conv_;
		snd_wnd = IKCP_WND_SND;
		rcv_wnd = IKCP_WND_RCV;
		rmt_wnd = IKCP_WND_RCV;
		mtu = IKCP_MTU_DEF;
		mss = mtu - IKCP_OVERHEAD;

		rx_rto = IKCP_RTO_DEF;
		rx_minrto = IKCP_RTO_MIN;
		interval = IKCP_INTERVAL;
		ts_flush = IKCP_INTERVAL;
		ssthresh = IKCP_THRESH_INIT;
		dead_link = IKCP_DEADLINK;
		buffer = new byte[(mtu + IKCP_OVERHEAD) * 3];
		output = output_;
	}

	// check the size of next message in the recv queue
	public int PeekSize()
	{
		if (0 == rcv_queue.Length)
		{
			return -1;
		}

		Segment seq = rcv_queue[0];

		if (0 == seq.frg)
		{
			return seq.data.Length;
		}

		if (rcv_queue.Length < seq.frg + 1)
		{
			return -1;
		}

		int length = 0;

		foreach (Segment item in rcv_queue)
		{
			length += item.data.Length;
			if (0 == item.frg)
			{
				break;
			}
		}

		return length;
	}

	// user/upper level recv: returns size, returns below zero for EAGAIN
	public int Recv(byte[] buffer)
	{
		if (0 == rcv_queue.Length)
		{
			return -1;
		}

		int peekSize = PeekSize();
		if (0 > peekSize)
		{
			return -2;
		}

		if (peekSize > buffer.Length)
		{
			return -3;
		}

		bool fast_recover = false;
		if (rcv_queue.Length >= rcv_wnd)
		{
			fast_recover = true;
		}

		// merge fragment.
		int count = 0;
		int n = 0;
		foreach (Segment seg in rcv_queue)
		{
			Array.Copy(seg.data, 0, buffer, n, seg.data.Length);
			n += seg.data.Length;
			count++;
			if (0 == seg.frg)
			{
				break;
			}
		}

		if (0 < count)
		{
			this.rcv_queue = slice(this.rcv_queue, count, this.rcv_queue.Length);
		}

		// move available data from rcv_buf -> rcv_queue
		count = 0;
		foreach (Segment seg in rcv_buf)
		{
			if (seg.sn == this.rcv_nxt && this.rcv_queue.Length < this.rcv_wnd)
			{
				this.rcv_queue = append(this.rcv_queue, seg);
				this.rcv_nxt++;
				count++;
			}
			else
			{
				break;
			}
		}

		if (0 < count)
		{
			this.rcv_buf = slice(this.rcv_buf, count, this.rcv_buf.Length);
		}

		// fast recover
		if (rcv_queue.Length < rcv_wnd && fast_recover)
		{
			this.probe |= IKCP_ASK_TELL;
		}

		return n;
	}

	// user/upper level send, returns below zero for error
	public int Send(byte[] bytes, int index, int length)
	{
		if (0 == bytes.Length)
		{
			return -1;
		}

		if (length == 0)
		{
			return -1;
		}

		int count = 0;

		if (length < mss)
		{
			count = 1;
		}
		else
		{
			count = (int)(length + this.mss - 1) / (int)this.mss;
		}

		if (255 < count)
		{
			return -2;
		}

		if (0 == count)
		{
			count = 1;
		}

		int offset = 0;

		for (int i = 0; i < count; i++)
		{
			int size = 0;
			if (length - offset > mss)
			{
				size = (int)this.mss;
			}
			else
			{
				size = length - offset;
			}

			Segment seg = new Segment(size);
			Array.Copy(bytes, offset + index, seg.data, 0, size);
			offset += size;
			seg.frg = (UInt32)(count - i - 1);
			snd_queue = append(snd_queue, seg);
		}

		return 0;
	}

	// update ack.
	private void update_ack(Int32 rtt)
	{
		if (0 == rx_srtt)
		{
			rx_srtt = (UInt32)rtt;
			rx_rttval = (UInt32)rtt / 2;
		}
		else
		{
			Int32 delta = (Int32)((UInt32)rtt - rx_srtt);
			if (0 > delta)
			{
				delta = -delta;
			}

			rx_rttval = (3 * rx_rttval + (uint)delta) / 4;
			rx_srtt = (UInt32)((7 * rx_srtt + rtt) / 8);
			if (rx_srtt < 1)
			{
				this.rx_srtt = 1;
			}
		}

		int rto = (int)(rx_srtt + _imax_(1, 4 * rx_rttval));
		rx_rto = _ibound_(rx_minrto, (UInt32)rto, IKCP_RTO_MAX);
	}

	private void shrink_buf()
	{
		if (snd_buf.Length > 0)
		{
			this.snd_una = this.snd_buf[0].sn;
		}
		else
		{
			this.snd_una = this.snd_nxt;
		}
	}

	private void parse_ack(UInt32 sn)
	{
		if (_itimediff(sn, snd_una) < 0 || _itimediff(sn, snd_nxt) >= 0)
		{
			return;
		}

		int index = 0;
		foreach (Segment seg in snd_buf)
		{
			if (sn == seg.sn)
			{
				snd_buf = append(slice(snd_buf, 0, index), slice(snd_buf, index + 1, snd_buf.Length));
				break;
			}
			seg.fastack++;

			index++;
		}
	}

	private void parse_una(UInt32 una)
	{
		int count = 0;
		foreach (Segment seg in snd_buf)
		{
			if (_itimediff(una, seg.sn) > 0)
			{
				count++;
			}
			else
			{
				break;
			}
		}

		if (0 < count)
		{
			this.snd_buf = slice(this.snd_buf, count, this.snd_buf.Length);
		}
	}

	private void ack_push(UInt32 sn, UInt32 ts)
	{
		acklist = append(acklist, new UInt32[2] { sn, ts });
	}

	private void ack_get(int p, ref UInt32 sn, ref UInt32 ts)
	{
		sn = acklist[p * 2 + 0];
		ts = acklist[p * 2 + 1];
	}

	private void parse_data(Segment newseg)
	{
		uint sn = newseg.sn;
		if (_itimediff(sn, rcv_nxt + rcv_wnd) >= 0 || _itimediff(sn, rcv_nxt) < 0)
		{
			return;
		}

		int n = rcv_buf.Length - 1;
		int after_idx = -1;
		bool repeat = false;
		for (int i = n; i >= 0; i--)
		{
			Segment seg = rcv_buf[i];
			if (seg.sn == sn)
			{
				repeat = true;
				break;
			}

			if (_itimediff(sn, seg.sn) > 0)
			{
				after_idx = i;
				break;
			}
		}

		if (!repeat)
		{
			if (after_idx == -1)
			{
				this.rcv_buf = append(new Segment[1] { newseg }, this.rcv_buf);
			}
			else
			{
				this.rcv_buf = append(slice(this.rcv_buf, 0, after_idx + 1),
				                      append(new Segment[1] { newseg }, slice(this.rcv_buf, after_idx + 1, this.rcv_buf.Length)));
			}
		}

		// move available data from rcv_buf -> rcv_queue
		int count = 0;
		foreach (Segment seg in rcv_buf)
		{
			if (seg.sn == this.rcv_nxt && this.rcv_queue.Length < this.rcv_wnd)
			{
				this.rcv_queue = append(this.rcv_queue, seg);
				this.rcv_nxt++;
				count++;
			}
			else
			{
				break;
			}
		}

		if (0 < count)
		{
			this.rcv_buf = slice(this.rcv_buf, count, this.rcv_buf.Length);
		}
	}

	// when you received a low level packet (eg. UDP packet), call it
	public int Input(byte[] data)
	{
		uint s_una = snd_una;
		if (data.Length < IKCP_OVERHEAD)
		{
			return 0;
		}

		int offset = 0;

		while (true)
		{
			UInt32 ts = 0;
			UInt32 sn = 0;
			UInt32 length = 0;
			UInt32 una = 0;
			UInt32 conv_ = 0;

			UInt16 wnd = 0;

			byte cmd = 0;
			byte frg = 0;

			if (data.Length - offset < IKCP_OVERHEAD)
			{
				break;
			}

			offset += ikcp_decode32u(data, offset, ref conv_);

			// 这里我做了修改，不判断两端kcp conv相等，因为客户端也需要一个socket支持多个client连接
			//if (conv != conv_)
			//	return -1;

			offset += ikcp_decode8u(data, offset, ref cmd);
			offset += ikcp_decode8u(data, offset, ref frg);
			offset += ikcp_decode16u(data, offset, ref wnd);
			offset += ikcp_decode32u(data, offset, ref ts);
			offset += ikcp_decode32u(data, offset, ref sn);
			offset += ikcp_decode32u(data, offset, ref una);
			offset += ikcp_decode32u(data, offset, ref length);

			if (data.Length - offset < length)
			{
				return -2;
			}

			switch (cmd)
			{
				case IKCP_CMD_PUSH:
				case IKCP_CMD_ACK:
				case IKCP_CMD_WASK:
				case IKCP_CMD_WINS:
					break;
				default:
					return -3;
			}

			rmt_wnd = wnd;
			parse_una(una);
			shrink_buf();

			if (IKCP_CMD_ACK == cmd)
			{
				if (_itimediff(current, ts) >= 0)
				{
					this.update_ack(_itimediff(this.current, ts));
				}

				parse_ack(sn);
				shrink_buf();
			}
			else if (IKCP_CMD_PUSH == cmd)
			{
				if (_itimediff(sn, rcv_nxt + rcv_wnd) < 0)
				{
					ack_push(sn, ts);
					if (_itimediff(sn, rcv_nxt) >= 0)
					{
						Segment seg = new Segment((int)length);
						seg.conv = conv_;
						seg.cmd = cmd;
						seg.frg = frg;
						seg.wnd = wnd;
						seg.ts = ts;
						seg.sn = sn;
						seg.una = una;

						if (length > 0)
						{
							Array.Copy(data, offset, seg.data, 0, length);
						}

						parse_data(seg);
					}
				}
			}
			else if (IKCP_CMD_WASK == cmd)
			{
				// ready to send back IKCP_CMD_WINS in Ikcp_flush
				// tell remote my window size
				probe |= IKCP_ASK_TELL;
			}
			else if (IKCP_CMD_WINS == cmd)
			{
				// do nothing
			}
			else
			{
				return -3;
			}

			offset += (int)length;
		}

		if (_itimediff(snd_una, s_una) > 0)
		{
			if (this.cwnd < this.rmt_wnd)
			{
				uint mss_ = this.mss;
				if (this.cwnd < this.ssthresh)
				{
					this.cwnd++;
					this.incr += mss_;
				}
				else
				{
					if (this.incr < mss_)
					{
						this.incr = mss_;
					}

					this.incr += mss_ * mss_ / this.incr + mss_ / 16;
					if ((this.cwnd + 1) * mss_ <= this.incr)
					{
						this.cwnd++;
					}
				}
				if (this.cwnd > this.rmt_wnd)
				{
					this.cwnd = this.rmt_wnd;
					this.incr = this.rmt_wnd * mss_;
				}
			}
		}

		return 0;
	}

	private Int32 wnd_unused()
	{
		if (rcv_queue.Length < rcv_wnd)
		{
			return (int)this.rcv_wnd - this.rcv_queue.Length;
		}

		return 0;
	}

	// flush pending data
	private void flush()
	{
		uint current_ = current;
		var buffer_ = buffer;
		int change = 0;
		int lost = 0;

		if (0 == updated)
		{
			return;
		}

		Segment seg = new Segment(0);
		seg.conv = conv;
		seg.cmd = IKCP_CMD_ACK;
		seg.wnd = (UInt32)wnd_unused();
		seg.una = rcv_nxt;

		// flush acknowledges
		int count = acklist.Length / 2;
		int offset = 0;
		for (int i = 0; i < count; i++)
		{
			if (offset + IKCP_OVERHEAD > mtu)
			{
				output(buffer, offset);
				//Array.Clear(buffer, 0, offset);
				offset = 0;
			}
			ack_get(i, ref seg.sn, ref seg.ts);
			offset += seg.encode(buffer, offset);
		}
		acklist = new UInt32[0];

		// probe window size (if remote window size equals zero)
		if (0 == rmt_wnd)
		{
			if (0 == probe_wait)
			{
				probe_wait = IKCP_PROBE_INIT;
				ts_probe = current + probe_wait;
			}
			else
			{
				if (_itimediff(current, ts_probe) >= 0)
				{
					if (probe_wait < IKCP_PROBE_INIT)
					{
						this.probe_wait = IKCP_PROBE_INIT;
					}

					probe_wait += probe_wait / 2;
					if (probe_wait > IKCP_PROBE_LIMIT)
					{
						this.probe_wait = IKCP_PROBE_LIMIT;
					}

					ts_probe = current + probe_wait;
					probe |= IKCP_ASK_SEND;
				}
			}
		}
		else
		{
			ts_probe = 0;
			probe_wait = 0;
		}

		// flush window probing commands
		if ((probe & IKCP_ASK_SEND) != 0)
		{
			seg.cmd = IKCP_CMD_WASK;
			if (offset + IKCP_OVERHEAD > (int)mtu)
			{
				output(buffer, offset);
				//Array.Clear(buffer, 0, offset);
				offset = 0;
			}
			offset += seg.encode(buffer, offset);
		}

		probe = 0;

		// calculate window size
		uint cwnd_ = _imin_(snd_wnd, rmt_wnd);
		if (0 == nocwnd)
		{
			cwnd_ = _imin_(this.cwnd, cwnd_);
		}

		count = 0;
		for (int k = 0; k < snd_queue.Length; k++)
		{
			if (_itimediff(snd_nxt, snd_una + cwnd_) >= 0)
			{
				break;
			}

			Segment newseg = snd_queue[k];
			newseg.conv = conv;
			newseg.cmd = IKCP_CMD_PUSH;
			newseg.wnd = seg.wnd;
			newseg.ts = current_;
			newseg.sn = snd_nxt;
			newseg.una = rcv_nxt;
			newseg.resendts = current_;
			newseg.rto = rx_rto;
			newseg.fastack = 0;
			newseg.xmit = 0;
			snd_buf = append(snd_buf, newseg);
			snd_nxt++;
			count++;
		}

		if (0 < count)
		{
			this.snd_queue = slice(this.snd_queue, count, this.snd_queue.Length);
		}

		// calculate resent
		uint resent = (UInt32)fastresend;
		if (fastresend <= 0)
		{
			resent = 0xffffffff;
		}

		uint rtomin = rx_rto >> 3;
		if (nodelay != 0)
		{
			rtomin = 0;
		}

		// flush data segments
		foreach (Segment segment in snd_buf)
		{
			bool needsend = false;
			int debug = _itimediff(current_, segment.resendts);
			if (0 == segment.xmit)
			{
				needsend = true;
				segment.xmit++;
				segment.rto = rx_rto;
				segment.resendts = current_ + segment.rto + rtomin;
			}
			else if (_itimediff(current_, segment.resendts) >= 0)
			{
				needsend = true;
				segment.xmit++;
				xmit++;
				if (0 == nodelay)
				{
					segment.rto += this.rx_rto;
				}
				else
				{
					segment.rto += this.rx_rto / 2;
				}

				segment.resendts = current_ + segment.rto;
				lost = 1;
			}
			else if (segment.fastack >= resent)
			{
				needsend = true;
				segment.xmit++;
				segment.fastack = 0;
				segment.resendts = current_ + segment.rto;
				change++;
			}

			if (needsend)
			{
				segment.ts = current_;
				segment.wnd = seg.wnd;
				segment.una = rcv_nxt;

				int need = IKCP_OVERHEAD + segment.data.Length;
				if (offset + need > mtu)
				{
					output(buffer, offset);
					//Array.Clear(buffer, 0, offset);
					offset = 0;
				}

				offset += segment.encode(buffer, offset);
				if (segment.data.Length > 0)
				{
					Array.Copy(segment.data, 0, buffer, offset, segment.data.Length);
					offset += segment.data.Length;
				}

				if (segment.xmit >= dead_link)
				{
					this.state = 0;
				}
			}
		}

		// flash remain segments
		if (offset > 0)
		{
			output(buffer, offset);
			//Array.Clear(buffer, 0, offset);
			offset = 0;
		}

		// update ssthresh
		if (change != 0)
		{
			uint inflight = snd_nxt - snd_una;
			ssthresh = inflight / 2;
			if (ssthresh < IKCP_THRESH_MIN)
			{
				this.ssthresh = IKCP_THRESH_MIN;
			}

			cwnd = ssthresh + resent;
			incr = cwnd * mss;
		}

		if (lost != 0)
		{
			ssthresh = cwnd / 2;
			if (ssthresh < IKCP_THRESH_MIN)
			{
				this.ssthresh = IKCP_THRESH_MIN;
			}

			cwnd = 1;
			incr = mss;
		}

		if (cwnd < 1)
		{
			cwnd = 1;
			incr = mss;
		}
	}

	// update state (call it repeatedly, every 10ms-100ms), or you can ask
	// ikcp_check when to call it again (without ikcp_input/_send calling).
	// 'current' - current timestamp in millisec.
	public void Update(UInt32 current_)
	{
		current = current_;

		if (0 == updated)
		{
			updated = 1;
			ts_flush = current;
		}

		int slap = _itimediff(current, ts_flush);

		if (slap >= 10000 || slap < -10000)
		{
			ts_flush = current;
			slap = 0;
		}

		if (slap >= 0)
		{
			ts_flush += interval;
			if (_itimediff(current, ts_flush) >= 0)
			{
				this.ts_flush = this.current + this.interval;
			}

			flush();
		}
	}

	// Determine when should you invoke ikcp_update:
	// returns when you should invoke ikcp_update in millisec, if there
	// is no ikcp_input/_send calling. you can call ikcp_update in that
	// time, instead of call update repeatly.
	// Important to reduce unnacessary ikcp_update invoking. use it to
	// schedule ikcp_update (eg. implementing an epoll-like mechanism,
	// or optimize ikcp_update when handling massive kcp connections)
	public UInt32 Check(UInt32 current_)
	{
		if (0 == updated)
		{
			return current_;
		}

		uint ts_flush_ = ts_flush;
		int tm_flush_ = 0x7fffffff;
		int tm_packet = 0x7fffffff;
		int minimal = 0;

		if (_itimediff(current_, ts_flush_) >= 10000 || _itimediff(current_, ts_flush_) < -10000)
		{
			ts_flush_ = current_;
		}

		if (_itimediff(current_, ts_flush_) >= 0)
		{
			return current_;
		}

		tm_flush_ = _itimediff(ts_flush_, current_);

		foreach (Segment seg in snd_buf)
		{
			int diff = _itimediff(seg.resendts, current_);
			if (diff <= 0)
			{
				return current_;
			}

			if (diff < tm_packet)
			{
				tm_packet = diff;
			}
		}

		minimal = tm_packet;
		if (tm_packet >= tm_flush_)
		{
			minimal = tm_flush_;
		}

		if (minimal >= interval)
		{
			minimal = (int)this.interval;
		}

		return current_ + (UInt32)minimal;
	}

	// change MTU size, default is 1400
	public int SetMtu(Int32 mtu_)
	{
		if (mtu_ < 50 || mtu_ < IKCP_OVERHEAD)
		{
			return -1;
		}

		var buffer_ = new byte[(mtu_ + IKCP_OVERHEAD) * 3];
		if (null == buffer_)
		{
			return -2;
		}

		mtu = (UInt32)mtu_;
		mss = mtu - IKCP_OVERHEAD;
		buffer = buffer_;
		return 0;
	}

	public int Interval(Int32 interval_)
	{
		if (interval_ > 5000)
		{
			interval_ = 5000;
		}
		else if (interval_ < 10)
		{
			interval_ = 10;
		}

		interval = (UInt32)interval_;
		return 0;
	}

	// fastest: ikcp_nodelay(kcp, 1, 20, 2, 1)
	// nodelay: 0:disable(default), 1:enable
	// interval: internal update timer interval in millisec, default is 100ms
	// resend: 0:disable fast resend(default), 1:enable fast resend
	// nc: 0:normal congestion control(default), 1:disable congestion control
	public int NoDelay(int nodelay_, int interval_, int resend_, int nc_)
	{
		if (nodelay_ > 0)
		{
			nodelay = (UInt32)nodelay_;
			if (nodelay_ != 0)
			{
				this.rx_minrto = IKCP_RTO_NDL;
			}
			else
			{
				this.rx_minrto = IKCP_RTO_MIN;
			}
		}

		if (interval_ >= 0)
		{
			if (interval_ > 5000)
			{
				interval_ = 5000;
			}
			else if (interval_ < 10)
			{
				interval_ = 10;
			}

			interval = (UInt32)interval_;
		}

		if (resend_ >= 0)
		{
			this.fastresend = resend_;
		}

		if (nc_ >= 0)
		{
			this.nocwnd = nc_;
		}

		return 0;
	}

	// set maximum window size: sndwnd=32, rcvwnd=32 by default
	public int WndSize(int sndwnd, int rcvwnd)
	{
		if (sndwnd > 0)
		{
			this.snd_wnd = (UInt32)sndwnd;
		}

		if (rcvwnd > 0)
		{
			this.rcv_wnd = (UInt32)rcvwnd;
		}

		return 0;
	}

	// get how many packet is waiting to be sent
	public int WaitSnd()
	{
		return snd_buf.Length + snd_queue.Length;
	}
}