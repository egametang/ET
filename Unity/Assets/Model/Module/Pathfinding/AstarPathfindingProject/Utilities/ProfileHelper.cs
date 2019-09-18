// Disable some warnings since this class compiles out large parts of the code depending on compiler directives
#pragma warning disable 0162
#pragma warning disable 0414
#pragma warning disable 0429
//#define PROFILE // Uncomment to enable profiling
//#define KEEP_SAMPLES
using System;
using System.Collections.Generic;

namespace Pathfinding {
	public class Profile {
		const bool PROFILE_MEM = false;

		public readonly string name;
		readonly System.Diagnostics.Stopwatch watch;
		int counter;
		long mem;
		long smem;

#if KEEP_SAMPLES
		List<float> samples = new List<float>();
#endif

		int control = 1 << 30;
		const bool dontCountFirst = false;

		public int ControlValue () {
			return control;
		}

		public Profile (string name) {
			this.name = name;
			watch = new System.Diagnostics.Stopwatch();
		}

		public static void WriteCSV (string path, params Profile[] profiles) {
#if KEEP_SAMPLES
			var s = new System.Text.StringBuilder();
			s.AppendLine("x, y");
			foreach (var profile in profiles) {
				for (int i = 0; i < profile.samples.Count; i++) {
					s.AppendLine(profile.name + ", " + profile.samples[i].ToString("R"));
				}
			}
			System.IO.File.WriteAllText(path, s.ToString());
#endif
		}

		public void Run (System.Action action) {
			Start();
			action();
			Stop();
		}

		[System.Diagnostics.ConditionalAttribute("PROFILE")]
		public void Start () {
			if (PROFILE_MEM) {
				smem = GC.GetTotalMemory(false);
			}
			if (dontCountFirst && counter == 1) return;
			watch.Start();
		}

		[System.Diagnostics.ConditionalAttribute("PROFILE")]
		public void Stop () {
			counter++;
			if (dontCountFirst && counter == 1) return;

			watch.Stop();
			if (PROFILE_MEM) {
				mem += GC.GetTotalMemory(false)-smem;
			}
#if KEEP_SAMPLES
			samples.Add((float)watch.Elapsed.TotalMilliseconds);
			watch.Reset();
#endif
		}

		[System.Diagnostics.ConditionalAttribute("PROFILE")]
		/** Log using Debug.Log */
		public void Log () {
			UnityEngine.Debug.Log(ToString());
		}

		[System.Diagnostics.ConditionalAttribute("PROFILE")]
		/** Log using System.Console */
		public void ConsoleLog () {
#if !NETFX_CORE || UNITY_EDITOR
			System.Console.WriteLine(ToString());
#endif
		}

		[System.Diagnostics.ConditionalAttribute("PROFILE")]
		public void Stop (int control) {
			counter++;
			if (dontCountFirst && counter == 1) return;

			watch.Stop();
			if (PROFILE_MEM) {
				mem += GC.GetTotalMemory(false)-smem;
			}

			if (this.control == 1 << 30) this.control = control;
			else if (this.control != control) throw new Exception("Control numbers do not match " + this.control + " != " + control);
		}

		[System.Diagnostics.ConditionalAttribute("PROFILE")]
		public void Control (Profile other) {
			if (ControlValue() != other.ControlValue()) {
				throw new Exception("Control numbers do not match ("+name + " " + other.name + ") " + ControlValue() + " != " + other.ControlValue());
			}
		}

		public override string ToString () {
			string s = name + " #" + counter + " " + watch.Elapsed.TotalMilliseconds.ToString("0.0 ms") + " avg: " + (watch.Elapsed.TotalMilliseconds/counter).ToString("0.00 ms");

			if (PROFILE_MEM) {
				s += " avg mem: " + (mem/(1.0*counter)).ToString("0 bytes");
			}
			return s;
		}
	}
}
