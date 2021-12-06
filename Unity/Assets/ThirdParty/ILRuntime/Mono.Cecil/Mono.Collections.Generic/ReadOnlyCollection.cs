//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ILRuntime.Mono.Collections.Generic {

	public sealed class ReadOnlyCollection<T> : Collection<T>, ICollection<T>, IList {

		static ReadOnlyCollection<T> empty;

		public static ReadOnlyCollection<T> Empty {
			get {
				if (empty != null)
					return empty;

				Interlocked.CompareExchange (ref empty, new ReadOnlyCollection<T> (), null);
				return empty;
			}
		}

		bool ICollection<T>.IsReadOnly {
			get { return true; }
		}

		bool IList.IsFixedSize {
			get { return true; }
		}

		bool IList.IsReadOnly {
			get { return true; }
		}

		ReadOnlyCollection ()
		{
		}

		public ReadOnlyCollection (T [] array)
		{
			if (array == null)
				throw new ArgumentNullException ();

			Initialize (array, array.Length);
		}

		public ReadOnlyCollection (Collection<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ();

			Initialize (collection.items, collection.size);
		}

		void Initialize (T [] items, int size)
		{
			this.items = new T [size];
			Array.Copy (items, 0, this.items, 0, size);
			this.size = size;
		}

		internal override void Grow (int desired)
		{
			throw new InvalidOperationException ();
		}

		protected override void OnAdd (T item, int index)
		{
			throw new InvalidOperationException ();
		}

		protected override void OnClear ()
		{
			throw new InvalidOperationException ();
		}

		protected override void OnInsert (T item, int index)
		{
			throw new InvalidOperationException ();
		}

		protected override void OnRemove (T item, int index)
		{
			throw new InvalidOperationException ();
		}

		protected override void OnSet (T item, int index)
		{
			throw new InvalidOperationException ();
		}
	}
}
