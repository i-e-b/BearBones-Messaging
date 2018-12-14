BearBones-Messaging
===================

<img src="https://github.com/i-e-b/BearBones-Messaging/raw/master/bonebear.png" width="169" height="184"/>

BearBones messaging: lower-level framework, part of a contract-interface based distributed event framework for .Net

Short-term tasks
----------------

* [x] Remove ServiceStack (it has gone proprietary)
* [x] Remove StructureMap
* [x] Move contract stack out of message body into BasicProperties
* [x] Remove assembly name from contract space (keep namespace)
* [ ] Expose raw data messaging / remove serialisation?
* [ ] Add group-name header to outgoing messages (optional?)
* [ ] Pickup limit (dead letter after multiple fails)
* [ ] Deadletter count function
* [ ] New NuGet package

Possible future features
------------------------

* Retry messages to go to end of queue (so the whole queue cycles if there are a few bad messages)
* RPC vote-to-consume pattern
