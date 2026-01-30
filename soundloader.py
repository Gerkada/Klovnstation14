#!/usr/bin/env python3
from pathlib import Path
import mutagen
from mutagen.mp3 import MP3
from mutagen.oggvorbis import OggVorbis
import sys

# Cross-platform paths using pathlib (works everywhere)
script_path = Path(__file__).parent
music_path = script_path / "Resources" / "Audio" / "Lobby"

# Verify music directory exists
if not music_path.exists():
    print(f"Error: Music directory not found: {music_path}")
    sys.exit(1)

# Cross-platform file filtering
lobbysongs = [
    f for f in music_path.iterdir()
    if f.is_file() and f.suffix.lower() in {'.mp3', '.ogg'}
]

print(f"Found {len(lobbysongs)} tracks")

# Sound collection (cross-platform dirs)
output_dir = script_path / "Resources" / "Prototypes" / "SoundCollections"
output_dir.mkdir(parents=True, exist_ok=True)
lobby_cfg = output_dir / "lobby.yml"

with open(lobby_cfg, 'w', newline='\n', encoding='utf-8') as f:
    f.write("- type: soundCollection\n")
    f.write("  id: LobbyMusic\n")
    f.write("  files:\n")
    for item in sorted(lobbysongs, key=lambda x: x.name.lower()):
        f.write(f"    - /Audio/Lobby/{item.name}\n")

# Jukebox file
jb_output_dir = script_path / "Resources" / "Prototypes" / "Catalog" / "Jukebox"
jb_output_dir.mkdir(parents=True, exist_ok=True)
jb_cfg = jb_output_dir / "Standard.yml"

with open(jb_cfg, 'w', newline='\n', encoding='utf-8') as f:
    for item in sorted(lobbysongs, key=lambda x: x.name.lower()):
        full_path = item

        # Robust metadata extraction with fallbacks
        title = item.stem  # Filename without extension

        try:
            if item.suffix.lower() == '.mp3':
                audio = MP3(full_path)
                title = str(audio.get('TIT2', [title])[0])
            elif item.suffix.lower() == '.ogg':
                audio = OggVorbis(full_path)
                title = str(audio.get('title', [title])[0])
        except Exception as e:
            print(f"Warning: Could not read metadata for {item.name}: {e}")
            # Keep filename fallback

        # Cross-platform safe ID: lowercase, spaces→_, alnum + _- only
        safe_id = ''.join(c for c in title.lower().replace(' ', '_')
                         if c.isalnum() or c in ('_', '-'))
        if not safe_id:
            safe_id = item.stem.lower()

        f.write("- type: jukebox\n")
        f.write(f"  id: {safe_id}\n")
        f.write(f"  name: {title}\n")
        f.write("  path:\n")
        f.write(f"    path: /Audio/Lobby/{item.name}\n")
        f.write("\n")

print(f"✓ Generated files:")
print(f"  → {lobby_cfg}")
print(f"  → {jb_cfg}")
