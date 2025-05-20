Read Only

Tools and environment

The task requires knowledge of C# programming language. It is up to candidate to decide how to provide the
task deliverables and instructions on how to run the code (if it is necessary).

System description and requirements

The system is designed to generate and use DISCOUNT codes. The system consists of a server-side and a client-
side. Communication depends on the protocol specified below.

The Task

Create server-side of the system.

Client-side is optional, but appreciated. Client code quality will not be evaluated. The server-side should
consist of:

• DISCOUNT code generation.

• DISCOUNT code usage.

• Communication between server and client.

Requirements

• DISCOUNT codes must remain between service restarts. Store them in persistent storage (db, file, etc.)

• The length of the DISCOUNT code is 7-8 characters during generation.

• DISCOUNT code must be generated randomly and cannot repeat.

• Generation could be repeated as many times as desired

• Maximum of 2 thousand DISCOUNT codes can be generated with single request.

• System must be capable of processing multiple requests in parallel.

• Unit tests (optional).
Protocol
Do not use WEB APIs (REST)
