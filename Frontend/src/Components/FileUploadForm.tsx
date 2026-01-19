import { useCallback, useState } from 'react';
import { useDropzone } from 'react-dropzone';

export const FileUploadForm = () => {
    const [file, setFile] = useState<File | null>(null);
    const [uploading, setUploading] = useState(false);

    const onDrop = useCallback((acceptedFiles: File[]) => {
        setFile(acceptedFiles[0]);
    }, []);

    const { getRootProps, getInputProps, isDragActive } = useDropzone({
        onDrop,
        accept: {
            'audio/*': ['.mp3'],
        },
        multiple: false
    });

    const handleUpload = async () => {
        if (!file) return;

        setUploading(true);
        const formData = new FormData();
        // formData.append('mediaFileFullName', file.name); // Имя для вашего эндпоинта
        formData.append('file', file); // Если будете передавать сам файл

        try {
            const uploadResponse = await fetch('http://localhost:5205/content/upload', {
                method: 'POST',
                body: formData
            });

            if (uploadResponse.ok) {
                const uploadResult = await uploadResponse.json();
                console.log("Файл сохранен:", uploadResult.detail);
                console.log("Создается плейлист...");

                const createPlaylistResponse = await fetch('http://localhost:5205/playlist/create', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json' 
                    },
                    body: JSON.stringify(uploadResult.detail) 
                });

                if (createPlaylistResponse.ok) {
                    const createPlaylistResult = await createPlaylistResponse.json();

                    if (createPlaylistResult.status == 200)
                        console.log("Плейлист создан");
                }
            }
        } catch (error) {
            console.error('Ошибка загрузки:', error);
        } finally {
            setUploading(false);
        }
    };

    return (
        <div className="flex flex-col items-center w-full max-w-md p-8 bg-white/10 backdrop-blur-2xl rounded-[2.5rem] border border-white/20 shadow-2xl">
            <h2 className="text-white text-2xl font-light mb-6">Загрузка аудио</h2>

            <div
                {...getRootProps()}
                className={`w-full p-10 border-2 border-dashed rounded-3xl transition-all cursor-pointer flex flex-col items-center justify-center
          ${isDragActive ? 'border-purple-400 bg-purple-400/10' : 'border-white/30 hover:border-white/60'}`}
            >
                <input {...getInputProps()} />

                <div className="text-4xl mb-4">
                    {isDragActive ? '📥' : '🎵'}
                </div>

                <p className="text-white/70 text-center text-sm">
                    {isDragActive
                        ? "Бросайте файл сюда..."
                        : "Перетащите аудио файл или кликните для выбора"}
                </p>
            </div>

            {file && (
                <div className="mt-6 w-full animate-in fade-in slide-in-from-bottom-4">
                    <div className="flex items-center p-3 bg-white/5 rounded-xl border border-white/10">
                        <span className="text-white/90 text-sm truncate flex-1">{file.name}</span>
                        <button
                            onClick={() => setFile(null)}
                            className="ml-2 text-white/50 hover:text-white"
                        >
                            ✕
                        </button>
                    </div>

                    <button
                        onClick={handleUpload}
                        disabled={uploading}
                        className="w-full mt-4 py-3 bg-purple-600 hover:bg-purple-500 disabled:bg-gray-600 text-white rounded-xl font-medium transition-colors shadow-lg shadow-purple-500/20"
                    >
                        {uploading ? "Обработка..." : "Создать HLS плейлист"}
                    </button>
                </div>
            )}
        </div>
    );
};