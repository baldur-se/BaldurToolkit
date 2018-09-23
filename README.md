BaldurToolkit
=============

Toolkit for creating .NET server applications.
Originally created as a core of the Baldur MMORPG server engine, now available as a set of independent components.



Warning! Work in progress!
--------------------------

This project is incomplete. Use at your own risk.

**Global TODO:**

 - *General core review*. Many parts of this project were ported from legacy code and require a code review.
 - *Documentation*. Both XML doc comments in the code and help pages.
 - *Tests*. Complete code coverage is a must.



Toolkit components
------------------

This toolkit contains multiple separate and (in most cases) independent components.
You can use any of them to build your application. For example, you can use only BaldurToolkit.Network package in your client-side application to communicate with the server.

### BaldurToolkit.App
This component represents the base concept of BaldurToolkit's app system, where an app is a service that can be started, do some job while it runs, and stopped on request. A cross-platform .NET alternative to Windows services and Unix daemons.

### BaldurToolkit.AppRunner
Base implementation of BaldurToolkit app host, which can run single BaldurToolkit app as a command-line application and supports XML app description files to build an app without compilation of separate executable file.

### BaldurToolkit.Cron
A simple task scheduler with cron-like interval syntax which can be used to execute a code in specified moments. Think of it like a small cron inside your app.

### BaldurToolkit.DataTables
A simple tool to create in-memory collections of structured data, and build simple and fast indexes over them.

### BaldurToolkit.Network
A powerful library that allows client-server network communication. It contains:

 - **Buffer pool** to make garbage collection more efficient and get rid of memory fragmentation.
 - **Abstract protocol system** which allows to define protocol of any type and use any serialization system for your network messages.
 - **Connection abstraction system** to easily create and manage connections, listen for incoming connections, and connection pooling.
 - **Powerful controller system** to build complex and extensible message handling system. Includes implementation for opcode-based packet handling system.

### BaldurToolkit.Network.Iocp
High-performance TCP/IP implementation of BaldurToolkit.Network.Connections system based on .NET Framework's asynchronous sockets (IOCP), but available only on full .NET Framework or Mono.

### BaldurToolkit.Network.Libuv
Cross-platform implementation of BaldurToolkit.Network.Connections system based on the libuv library.


