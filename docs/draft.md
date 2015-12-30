# Draft v1

The protocol is the product of a wish to transmit JSON messages between computers, without installing or setting up heavyweight platforms or protocols. 
SSP is lightweight, binary and fixed in structure. Authentication is facilitated per packet, the user can choose to define their
own authentication system if they please.

# Protocol

The packet structure: 
```c
struct {
  char magic[5];
  uint8_t opcode;
  int32_t length;
  uint16_t sequence;
  char reserved[4];
  char token[32];
  char service[48];
  uint8_t payload[length];
}
```

## Magic

The magic is used to diffrenciate the packet from other protocols, the value should always be `JSPKZ`.

## Opcode

The opcode determines the type of packet, there are currently three opcodes in use.

* Internal (0x00)
* Message (0x01)
* Request (0x02)

## Sequence

The sequence is a unsigned 16-bit integer which is incremented locally each time a new packet is created, rolling back to zero once
it reached the maximum size. The sequence is used when replying to `Message` packets, allowing the response to be directed to the
correct handler when operations are run asycronously or with the same service name.

## Token

The token is used to authenticate packets, the maximum size of the string is 32 characters. In private systems, this value should
be used as a session cookie once authentication has been executed on a login service.

## Service

The service string specifies the service this packet is requesting to, all services prefixed by `__` are reserved for the protocol,
with current reservations being:

* `__KeepAlive`
* `__Disconnect`
* `__List`

## Payload

The payload is the contents of the transaction, this should be a UTF-8 JSON string.