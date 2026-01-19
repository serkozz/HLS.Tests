import React, { useEffect, useRef } from 'react';
import Hls from 'hls.js';

interface AudioPlayerProps {
  src: string; // URL to .m3u8 file
}

const HlsAudioPlayer: React.FC<AudioPlayerProps> = ({ src }) => {
  const audioRef = useRef<HTMLAudioElement>(null);

  useEffect(() => {
    const audio = audioRef.current;

    if (!audio) return;

    audio.volume = 0.2;

    let hls: Hls | null = null;

    if (Hls.isSupported()) {
      hls = new Hls({
        maxBufferLength: 30,
        enableWorker: true,
      });

      hls.loadSource(src);
      hls.attachMedia(audio);

      hls.on(Hls.Events.ERROR, (_, data) => {
        if (data.fatal) {
          switch (data.type) {
            case Hls.ErrorTypes.NETWORK_ERROR:
              hls?.startLoad();
              break;
            case Hls.ErrorTypes.MEDIA_ERROR:
              hls?.recoverMediaError();
              break;
            default:
              hls?.destroy();
              break;
          }
        }
      });
    }

    // Native support (Safari/iOS)
    else if (audio.canPlayType('application/vnd.apple.mpegurl')) {
      audio.src = src;
    }

    return () => {
      if (hls) {
        hls.destroy();
      }
    };
  }, [src]);

  return (
    <div className="flex flex-col items-center w-full max-w-md p-8 bg-white/10 backdrop-blur-2xl rounded-[2.5rem] border border-white/20 shadow-2xl">
      <h2 className="text-white text-2xl font-light mb-6">Стриминг аудио</h2>
      <audio
        ref={audioRef}
        controls
        autoPlay
      />
    </div>
  );
};

export default HlsAudioPlayer;