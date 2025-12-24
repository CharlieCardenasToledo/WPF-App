# Cómo crear una Release en GitHub

Sigue estos pasos para publicar el ejecutable en GitHub Releases:

## 1. Ve a tu repositorio en GitHub
https://github.com/CharlieCardenasToledo/WPF-App

## 2. Haz clic en "Releases" (lado derecho)
O ve directamente a: https://github.com/CharlieCardenasToledo/WPF-App/releases

## 3. Haz clic en "Create a new release"

## 4. Completa el formulario:

### Tag version:
```
v1.0.0
```

### Release title:
```
WingetUpdater v1.0.0 - Initial Release
```

### Description:
Copia y pega el contenido de `RELEASE_NOTES.md`

### Attach files:
Arrastra el archivo:
```
WingetUpdater\WingetUpdater-v1.0.0-win-x64.zip
```

## 5. Marca como "Latest release" ✅

## 6. Haz clic en "Publish release"

---

## ¡Listo! 🎉

Ahora los usuarios pueden descargar el ejecutable desde:
https://github.com/CharlieCardenasToledo/WPF-App/releases/tag/v1.0.0

El archivo ZIP contiene:
- `WingetUpdater.exe` (328 KB) - Ejecutable principal
- `WingetUpdater.pdb` - Símbolos de depuración (opcional, puedes excluirlo)
