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
* [x] Expose raw data messaging / remove serialisation?
* [x] Add group-name header to outgoing messages (optional?)
* [x] New NuGet package -- https://www.nuget.org/packages/BearBonesMessaging
* [x] Tests around TTL restriction and policy (per message TTL? or per queue?)
* [ ] Deadletter count function (message push-back)
* [ ] Required correlation id?

Possible future features
------------------------

These might be for other systems that build on this basis

* Pickup limit (dead letter after multiple fails)
* Retry messages to go to end of queue (so the whole queue cycles if there are a few bad messages)
* RPC vote-to-consume pattern
