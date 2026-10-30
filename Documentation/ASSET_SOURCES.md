# Asset sources and production notes

## Visuals and references

Animal sprites, environments, staff, demon, ark and title art were generated for this project using image generation tools and integrated into Unity. They are not presented as entirely hand-drawn artwork.

Design research included *Chants of Sennaar* for colors, silhouettes and architectural atmosphere; FGO April Fools games for short sessions, collection and progression; and *Arknights* for visual hierarchy. Existing character artwork, game screenshots, music and effects from these games were not imported as game assets.

Lasengle's [Fate/Dream Striker announcement](https://www.lasengle.co.jp/news/news-games/20240401_441620/) is an official reference for the short-event collection study. The management-game adaptation is this project's decision, not an endorsement or a claim that Lasengle developed it.

## Audio and font

Music and effects use project-specific programmatic synthesis. Selected `compose-v5.py`, `compose-weather-v6.py` and score JSON files preserve generation material; `ArkSound.cs` handles playback. No external commercial music samples or full voice recordings are included. Regenerating audio requires Python and NumPy; playing the project does not.

Noto Sans JP: [Google Fonts source](https://github.com/google/fonts/tree/main/ofl/notosansjp). Its SIL Open Font License is at `Assets/Resources/Fonts/OFL.txt`. This font license does not apply to the rest of the project.

## AI assistance and rights

AI assistance covered concept and Japanese text drafts, generated visuals, code, audio synthesis programs, build/validation tooling and presentation preparation. The creator supplied direction, experience goals and revision decisions. No independent third-party user research or entirely unaided production is claimed.

This repository has no blanket open-source license. Third-party materials retain their own licenses. Public availability alone does not authorize redistribution of all code and artwork.
