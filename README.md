# Convert2MP4 - Self-use tool convert any video file to MP4



## Requirement

`ffmpeg.exe` - in `Path` env or in the same dir of `Convert2MP4.exe`

`.NET Core 3.1`



## Configure

1. Change [those lines](Convert2MP4/Program.cs#L38-L42).
2. If you want to add more format support, add your file extension into `settings.Include`.
3. If you want to keep specific file, change your `settings.Exclude`.



## Usage

### Step 1:

- Drag & drop any `.mkv` / `.flv` files (*[OBS](https://obsproject.com/)*, *[Bilibili Live Recorder](https://rec.danmuji.org/)*) into `.exe` file.

- `Win + R` and run `shell:sendto`, copy shortcut into it.

    After select multiple files, right click on it, and choose `Send to -> Convert2MP4`.

    If you want to cleanup the record files days ago, copy another shortcut and add `-cm<daytoclean>` like `-cm3`, or `-cm` to use the default days in `settings.CleanupDays`, into shortcut's `Properties -> Target`.

     - By directly selecting files, `Include` and `Exclude` **are ignored**.

     - By selecting a folder, `Include` and `Exclude` **will filter which file to proceed** and which will not.

     - Clenup mode will **always perform** `Exclude`.

- Directly open `.exe` file, drag & drop file in it or copy file path, one file per line, leave blank to start converting

### Step 2:

After all convert task complete, you will get prompt whether delete the original file, enter `y` or `yes` to do so.
