https://ffmpeg.org/ffmpeg-filters.html#v360

ffmpeg -i GS010554.360 -c:v hevc_nvenc -t 00:00:10 -vf v360=output=equirect GS010554.mp4

-map 0:0 -map 0:1 -map 0:5 <- select the right streams

ffmpeg -f concat -i files.txt -c copy output.mp4