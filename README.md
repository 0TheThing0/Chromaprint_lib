# AudioSnap — audio metadata at your fingertips

AudioSnap is a project that strives to make audio metadata fetching easier by utilizing audio fingerprinting algorithms and making use of extensive audio metadata storages. The primary goal of this project was to get familiar with audio recognition algorithms and techniques.  

AudioSnap project is composed of 3 key components:
- [AudioSnap microservice] — microservice to recognize fingertips and fetch metadata from services
- [AudioSnap client] — utilizes this microservice to fetch audio metadata
- Chromaprint library — the current repository

The inspiration for the project comes from [this article][chromaprint article], that goes deep into audio recognition algorithms that formed the basis of the following web APIs our project relies on:
- [AcoustID] — recognize audio by its fingerprint
- [MusicBrainz] — fetch audio metadata by its ID, relies on AcoustID
- [CoverArtArchive] — fetch link to audio cover art, relied on MusicBrainz/AcoustID

## Disclaimer

> Although **there ARE solutions to the problem** provided by AcoustID and MusicBrainz themselves (e.g. [MusicBrainz Picard]), our project's goal was to — again — get familiar with the audio fingerprinting algorithms and build something that can be used as a convenient mean of providing metadata to audio files, analagous to solutions provided by MusicBrainz and AcoustID.  
Our goal was not to build a counterpart OR competitor to these solutions by any means, as our solution strongly relies on their services and their audio fingerprinting algorithms. Neither was our goal to monetise the application, even though it is allowed on a paid basis.  
Same goes to the rewritten [open-source chromaprint library][original chromaprint library], which is basically the implementation of the audio fingerprinting algorithms described in [this article][chromaprint article]. The goal was to get familiar with the algorithms, and we believe that the best way to learn is to practise.
>
# Notes
This library is used both in [client part] and [server part] of our application. Note, that library uses NAudio to process audiofiles and NAudio relies on API calls to DirectSound. That fact creates problems when using this library on platforms other than Windows.
Our microservice doesn't use functions with NAudio dependencies, making it suitable for use on Linux and other systems. But our client does. Well, it will run on Linux, but trying to process any file will throw an exception.

[AudioSnap microservice]: <https://github.com/SignificantNose/AudioSnapServer>
[AudioSnap client]: <https://github.com/0TheThing0/AvaloniaAudioSna>
[Original chromaprint library]: <https://github.com/acoustid/chromaprint/tree/master>
[Chromaprint article]: <https://oxygene.sk/2011/01/how-does-chromaprint-work/>

[AcoustID]: <https://acoustid.org/>

[MusicBrainz]: <https://musicbrainz.org/>

[CoverArtArchive]: <https://coverartarchive.org/>

[MusicBrainz Picard]: <https://picard.musicbrainz.org/>
