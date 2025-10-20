# UnityMMO_Setup

## Summary

This is intended for the startup of any type of Unity game implmenting multiplayer features. It features basic elements including a object that you can click to update a server wide variable, usernames that sync across the network, and a basic UI to join and host games.


## Set up Unity

Go to cloud.unity.com
+ Set up an organization if you have not already

#### Configure Project Settings

Edit -> Project Settings -> Services
select organization, create new cloud project, link project


## Unity Server developer notes
##### Things to keep in mind while working

+ In any type Network Behavior script, this is the order of how these functions are ran: 1. Awake, could be used for subscribing to network variables, 2. OnNetworkSpawn, could be used for checking for existing information stored on the network, 3. Start, could be used for owner only code (stuff that should only happen to that client)