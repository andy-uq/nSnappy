using System;
using System.Runtime.InteropServices;

namespace NSpanny
{
	public ref struct ReadOnlyPointer
	{
		private readonly ReadOnlySpan<byte> _buffer;
		private readonly ReadOnlySpan<byte> _data;
		private readonly int _position;
		private readonly string _name;

		public ReadOnlyPointer(ReadOnlySpan<byte> buffer, int position = 0, string name = null)
		{
			_buffer = buffer;
			_position = position;
			_name = name;

			_data = buffer.Slice(_position);
		}

		public ReadOnlyPointer(ReadOnlyPointer pointer) : this()
		{
			_buffer = pointer._buffer;
			_position = pointer._position;
			_name = pointer._name;
			_data = pointer._data;
		}

		public static ReadOnlyPointer operator +(ReadOnlyPointer pointer, int value)
		{
			return new ReadOnlyPointer(pointer._buffer, pointer._position + value, pointer._name);
		}

		public static ReadOnlyPointer operator -(ReadOnlyPointer pointer, int value)
		{
			return new ReadOnlyPointer(pointer._buffer, pointer._position - value, pointer._name);
		}

		public static ReadOnlyPointer operator +(ReadOnlyPointer pointer, uint value)
		{
			return new ReadOnlyPointer(pointer._buffer, checked((int) (pointer._position + value)), pointer._name);
		}

		public static implicit operator int(ReadOnlyPointer pointer) => pointer._position;

		public static explicit operator ushort(ReadOnlyPointer pointer) => ((ReadOnlySpan<ushort>)pointer)[0];
		public static explicit operator uint(ReadOnlyPointer pointer) => ((ReadOnlySpan<uint>)pointer)[0];
		public static explicit operator ulong(ReadOnlyPointer pointer) => ((ReadOnlySpan<ulong>)pointer)[0];
				
		public static explicit operator ReadOnlySpan<ushort>(ReadOnlyPointer pointer) => MemoryMarshal.Cast<byte, ushort>(pointer._data);
		public static explicit operator ReadOnlySpan<uint>(ReadOnlyPointer pointer) => MemoryMarshal.Cast<byte, uint>(pointer._data);
		public static explicit operator ReadOnlySpan<ulong>(ReadOnlyPointer pointer) => MemoryMarshal.Cast<byte, ulong>(pointer._data);

		public byte this[int offset] => _data[offset];

		public static bool operator ==(ReadOnlyPointer left, ReadOnlyPointer right)
		{
			return left._data == right._data;
		}

		public static bool operator !=(ReadOnlyPointer left, ReadOnlyPointer right)
		{
			return left._data != right._data;
		}

		public override string ToString()
		{
			var name = _name ?? "<???>";
			return _position == 0
				? string.Format("{0}[{1}]",     name, _buffer.Length)
				: string.Format("{0}[{1}]+{2}", name, _buffer.Length, _position);
		}

		public void CopyTo(Span<byte> destination)
		{
			_data
				.Slice(0, destination.Length)
				.CopyTo(destination);
		}
	}


	public ref struct Pointer
	{
		private readonly Span<byte> _buffer;
		private readonly Span<byte> _data;
		private readonly string _name;
		private readonly int _position;

		public Pointer(Pointer copyFrom)
		{
			_buffer = copyFrom._buffer;
			_position = copyFrom._position;
			_name = copyFrom._name;
			_data = copyFrom._data;
		}

		public Pointer(Span<byte> buffer, int position = 0, string name = null)
		{
			_buffer = buffer;
			_position = position;
			_name = name;
			_data = buffer.Slice(position);
		}

		public static implicit operator int(Pointer pointer) => pointer._position;

		public static implicit operator ReadOnlyPointer(Pointer pointer) => new ReadOnlyPointer(pointer._buffer, pointer._position, pointer._name);

		public static Pointer operator +(Pointer pointer, int value)
		{
			return new Pointer(pointer._buffer, pointer._position + value, pointer._name);
		}

		public static Pointer operator +(Pointer pointer, uint value)
		{
			return new Pointer(pointer._buffer, (int)(pointer._position + value), pointer._name);
		}

		public static Pointer operator -(Pointer pointer, int value)
		{
			return new Pointer(pointer._buffer, pointer._position - value, pointer._name);
		}

		public static explicit operator ushort(Pointer pointer) => ((Span<ushort>)pointer)[0];
		public static explicit operator uint(Pointer pointer) => ((Span<uint>)pointer)[0];
		public static explicit operator ulong(Pointer pointer) => ((Span<ulong>)pointer)[0];
				
		public static explicit operator Span<ushort>(Pointer pointer) => MemoryMarshal.Cast<byte, ushort>(pointer._data);
		public static explicit operator Span<uint>(Pointer pointer) => MemoryMarshal.Cast<byte, uint>(pointer._data);
		public static explicit operator Span<ulong>(Pointer pointer) => MemoryMarshal.Cast<byte, ulong>(pointer._data);

		public byte this[int offset]
		{
			get => _data[offset];
			set => _data[offset] = value;
		}

		public void Copy(Pointer source, int length)
		{
			source.CopyTo(_data.Slice(0, length));
		}

		private void CopyTo(Span<byte> destination)
		{
			_data
				.Slice(0, destination.Length)
				.CopyTo(destination);
		}

		public void Copy(ReadOnlyPointer source, int length)
		{
			source.CopyTo(_data.Slice(0, length));
		}

		public override string ToString()
		{
			var name = _name ?? "<???>";
			return _position == 0
			       	? string.Format("{0}[{1}]", name, _buffer.Length)
			       	: string.Format("{0}[{1}]+{2}", name, _buffer.Length, _position);
		}

		public static bool operator ==(Pointer left, Pointer right)
		{
			return left._data == right._data;
		}

		public static bool operator !=(Pointer left, Pointer right)
		{
			return left._data != right._data;
		}
	}
}