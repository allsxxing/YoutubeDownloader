#!/usr/bin/env bash
#
# extract_clip.sh - YouTube clip extraction tool
# Requires: yt-dlp, ffmpeg
#
# Usage:
#   ./extract_clip.sh --url URL --start HH:MM:SS --end HH:MM:SS [options]
#
# Options:
#   --url URL          YouTube video URL (required)
#   --start TIME       Start timestamp, e.g. 11:25 or 1:02:30 (required)
#   --end TIME         End timestamp (required)
#   --output DIR       Output directory (default: ./clips)
#   --format FMT       Output format: mp4, mkv, webm (default: mp4)
#   --quality QUAL     Video quality: best, 1080, 720, 480 (default: best)
#   --name NAME        Custom output filename (without extension)
#   --dry-run          Validate inputs without downloading
#   --verbose          Enable verbose output
#   --help             Show this help message

set -euo pipefail

# ---------- defaults ----------
OUTPUT_DIR="./clips"
LOG_DIR="./logs"
FORMAT="mp4"
QUALITY="best"
CUSTOM_NAME=""
DRY_RUN=false
VERBOSE=false
URL=""
START=""
END=""

# ---------- colors ----------
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

# ---------- helpers ----------
log()  { echo -e "${CYAN}[INFO]${NC} $*"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $*"; }
err()  { echo -e "${RED}[ERROR]${NC} $*" >&2; }
ok()   { echo -e "${GREEN}[OK]${NC} $*"; }

usage() {
    sed -n '/^# Usage:/,/^$/p' "$0" | sed 's/^# \?//'
    exit 0
}

# Normalize timestamp to HH:MM:SS
normalize_ts() {
    local ts="$1"
    local parts
    IFS=':' read -ra parts <<< "$ts"
    case ${#parts[@]} in
        2) printf "%02d:%02d:%02d" 0 "${parts[0]}" "${parts[1]}" ;;
        3) printf "%02d:%02d:%02d" "${parts[0]}" "${parts[1]}" "${parts[2]}" ;;
        *) echo "INVALID"; return 1 ;;
    esac
}

# Convert HH:MM:SS to total seconds
ts_to_seconds() {
    local ts="$1"
    local parts
    IFS=':' read -ra parts <<< "$ts"
    echo $(( ${parts[0]} * 3600 + ${parts[1]} * 60 + ${parts[2]} ))
}

# ---------- parse args ----------
while [[ $# -gt 0 ]]; do
    case "$1" in
        --url)      URL="$2"; shift 2 ;;
        --start)    START="$2"; shift 2 ;;
        --end)      END="$2"; shift 2 ;;
        --output)   OUTPUT_DIR="$2"; shift 2 ;;
        --format)   FORMAT="$2"; shift 2 ;;
        --quality)  QUALITY="$2"; shift 2 ;;
        --name)     CUSTOM_NAME="$2"; shift 2 ;;
        --dry-run)  DRY_RUN=true; shift ;;
        --verbose)  VERBOSE=true; shift ;;
        --help|-h)  usage ;;
        *)          err "Unknown option: $1"; usage ;;
    esac
done

# ---------- validate ----------
ERRORS=0

if [[ -z "$URL" ]]; then
    err "Missing --url"; ERRORS=$((ERRORS + 1))
fi
if [[ -z "$START" ]]; then
    err "Missing --start"; ERRORS=$((ERRORS + 1))
fi
if [[ -z "$END" ]]; then
    err "Missing --end"; ERRORS=$((ERRORS + 1))
fi

if ! command -v yt-dlp &>/dev/null; then
    err "yt-dlp not found. Install: brew install yt-dlp (macOS) or pip install yt-dlp"
    ERRORS=$((ERRORS + 1))
fi
if ! command -v ffmpeg &>/dev/null; then
    err "ffmpeg not found. Install: brew install ffmpeg (macOS) or apt install ffmpeg"
    ERRORS=$((ERRORS + 1))
fi

if [[ $ERRORS -gt 0 ]]; then
    exit 1
fi

# Normalize timestamps
START=$(normalize_ts "$START")
END=$(normalize_ts "$END")

START_SEC=$(ts_to_seconds "$START")
END_SEC=$(ts_to_seconds "$END")
DURATION=$((END_SEC - START_SEC))

if [[ $DURATION -le 0 ]]; then
    err "End time must be after start time (got duration=${DURATION}s)"
    exit 1
fi

if [[ $DURATION -gt 600 ]]; then
    warn "Clip duration is ${DURATION}s (>10 min). This may produce a large file."
fi

# Quality mapping for yt-dlp format selection
case "$QUALITY" in
    best) YT_FORMAT="bestvideo+bestaudio/best" ;;
    1080) YT_FORMAT="bestvideo[height<=1080]+bestaudio/best[height<=1080]" ;;
    720)  YT_FORMAT="bestvideo[height<=720]+bestaudio/best[height<=720]" ;;
    480)  YT_FORMAT="bestvideo[height<=480]+bestaudio/best[height<=480]" ;;
    *)    err "Unknown quality: $QUALITY (use: best, 1080, 720, 480)"; exit 1 ;;
esac

# ---------- setup ----------
mkdir -p "$OUTPUT_DIR" "$LOG_DIR"

LOGFILE="${LOG_DIR}/extract_$(date +%Y%m%d_%H%M%S).log"

# Fetch video title for naming
log "Fetching video metadata..."
VIDEO_TITLE=$(yt-dlp --get-title "$URL" 2>/dev/null || echo "unknown_video")
VIDEO_ID=$(yt-dlp --get-id "$URL" 2>/dev/null || echo "unknown_id")

# Sanitize title for filename
SAFE_TITLE=$(echo "$VIDEO_TITLE" | tr -cd '[:alnum:] _-' | tr ' ' '_' | cut -c1-60)

if [[ -n "$CUSTOM_NAME" ]]; then
    OUTFILE="${OUTPUT_DIR}/${CUSTOM_NAME}.${FORMAT}"
else
    OUTFILE="${OUTPUT_DIR}/${SAFE_TITLE}_${START//:/}-${END//:/}.${FORMAT}"
fi

# ---------- summary ----------
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
log "Clip Extraction Summary"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Video:    $VIDEO_TITLE"
echo "  ID:       $VIDEO_ID"
echo "  Start:    $START"
echo "  End:      $END"
echo "  Duration: ${DURATION}s"
echo "  Quality:  $QUALITY"
echo "  Format:   $FORMAT"
echo "  Output:   $OUTFILE"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

if $DRY_RUN; then
    ok "Dry run complete. All validations passed."
    # Output structured JSON for programmatic use
    cat <<ENDJSON
{
  "status": "dry_run_ok",
  "video_title": "$VIDEO_TITLE",
  "video_id": "$VIDEO_ID",
  "start": "$START",
  "end": "$END",
  "duration_seconds": $DURATION,
  "quality": "$QUALITY",
  "format": "$FORMAT",
  "output_path": "$OUTFILE"
}
ENDJSON
    exit 0
fi

# ---------- download + extract ----------
TMPFILE=$(mktemp "/tmp/yt_clip_XXXXXX.${FORMAT}")
trap 'rm -f "$TMPFILE"' EXIT

log "Downloading full video stream..."
{
    if $VERBOSE; then
        yt-dlp -f "$YT_FORMAT" --merge-output-format "$FORMAT" -o "$TMPFILE" "$URL" 2>&1 | tee -a "$LOGFILE"
    else
        yt-dlp -f "$YT_FORMAT" --merge-output-format "$FORMAT" -o "$TMPFILE" "$URL" >> "$LOGFILE" 2>&1
    fi
} || {
    # Fallback: try downloading section directly (yt-dlp 2023+)
    log "Full download failed, trying section download..."
    yt-dlp -f "$YT_FORMAT" --merge-output-format "$FORMAT" \
        --download-sections "*${START}-${END}" \
        -o "$TMPFILE" "$URL" >> "$LOGFILE" 2>&1
}

if [[ ! -f "$TMPFILE" ]] || [[ ! -s "$TMPFILE" ]]; then
    err "Download failed. Check $LOGFILE for details."
    exit 2
fi

log "Extracting clip [${START} -> ${END}] (${DURATION}s)..."
ffmpeg -y -i "$TMPFILE" \
    -ss "$START" -to "$END" \
    -c:v libx264 -c:a aac \
    -movflags +faststart \
    "$OUTFILE" >> "$LOGFILE" 2>&1

if [[ ! -f "$OUTFILE" ]] || [[ ! -s "$OUTFILE" ]]; then
    err "FFmpeg extraction failed. Check $LOGFILE for details."
    exit 3
fi

# ---------- result ----------
FILESIZE=$(du -h "$OUTFILE" | cut -f1)
CODEC_INFO=$(ffprobe -v quiet -show_entries stream=codec_name -of csv=p=0 "$OUTFILE" 2>/dev/null | head -2 | tr '\n' ',')

echo ""
ok "Clip saved successfully!"
echo ""

cat <<ENDJSON
{
  "status": "success",
  "video_title": "$VIDEO_TITLE",
  "video_id": "$VIDEO_ID",
  "start": "$START",
  "end": "$END",
  "duration_seconds": $DURATION,
  "output_path": "$OUTFILE",
  "file_size": "$FILESIZE",
  "codecs": "$CODEC_INFO",
  "log_file": "$LOGFILE"
}
ENDJSON
