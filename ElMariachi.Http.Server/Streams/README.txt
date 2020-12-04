HTTP streams considerations for developpers

The HTTP protocol is built such that it is always possible to determine if there are some remaining bytes to read or not.

For example:
	-> When HTTP method is to be read, you know that you should read until you reach a space char.
	-> When HTTP headers are to be read, you know that you should read until you reach a "\r\n" sequence.
	-> When the stream is chunked, you know that you should read until you reach the "0\r\n\r\n" sequence.
	-> When the headers contains the «content-length» header, you know that you should read until this number of bytes is reached.

It is also important to keep in mind that the behavior of the «System.Net.Sockets.NetworkStream» class (base stream class provided by the networdk socket)
is blocking until at least one byte is available for reading.

HTTP input streams implementation golden rules:
	-> You should throw a StreamEndException each time you know that you expect some more bytes to read but the reader returned 0 bytes
	-> If the request number of bytes to read is 0 (count argument of Read method), return 0.
