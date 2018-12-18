BearBones-Messaging
===================

<img src="https://github.com/i-e-b/BearBones-Messaging/raw/master/bonebear.png" width="169" height="184"/>

BearBones messaging: lower-level framework, part of a contract-interface based distributed event framework for .Net

Features
--------

* Contract stack in message BasicProperties
* Exposes raw data messaging or provides serialisation
* Add sender group-name header to outgoing messages
* NuGet package -- https://www.nuget.org/packages/BearBonesMessaging
* Queue TTL restriction and policy
    - Per app-group expiry
    - VHost-wide monitoring endpoint
* Optional correlation id, generate new GUID if not given
* Exposes some user management endpoints
    - Create limited user (write/read, but no manage)
    - Delete user
    - Connection string for user -- to expose to registrants

Possible future features
------------------------

These might be for other systems that build on this basis

* Pickup limit (dead letter after multiple fails)
* Retry messages to go to end of queue (so the whole queue cycles if there are a few bad messages)
* RPC vote-to-consume pattern
