# DeckSwipe

**Note from @outfrost**: I made this for my internship in 2018, and the code was sound to me at the time. I wanted to work on it a bit more, but other things took priority, and eventually I lost interest. Still, the repo has gathered some interest over the years, mainly (as far as I can tell) among folks learning how to make games, so I'm keeping it around for anyone who'd like to learn from it or make something with it.

## About

This is a skeleton for a simple card game. There are 4 gameplay resources (predefined as Coal, Food, Health, Hope), each contributing to the chances of survival for the player's city. Choices that the player makes through swiping each card left or right will influence those resources in various ways. If any one of the resources depletes (reaches zero), the game is lost and reset. The player's objective is to make decisions such that depletion doesn't happen, and the city survives, for as long as they can manage.

The core mechanics and visuals are heavily based on _Reigns_, and a clone, _Lapse: The Forgotten Future_. The sample content is mostly inspired by _Frostpunk_ and its neverending winter.

Created with Unity on Linux, primarily targetting Android.

![Screen capture of the game running on Android](screencap-android.gif)

## Development

This project is archived. If you wish to make use of the repo, I recommend forking or copying, and adapting it to your needs.

## License

All content published in this repository, be it software, in source code or binary form, or other works, is released under the MIT License, as documented in [LICENSE](./LICENSE), with the following exceptions:

* **TextMesh Pro**

	Bundled with Unity and distributed on Unity license terms

	https://unity3d.com/legal

	Files:

	* `DeckSwipe/Assets/Dependencies/TextMesh Pro/*`

