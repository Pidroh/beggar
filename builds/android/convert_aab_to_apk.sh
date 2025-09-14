#!/bin/bash

# Convert AAB to APK using bundletool
# Usage: ./convert_aab_to_apk.sh <aab_file>

if [ $# -eq 0 ]; then
    echo "Usage: $0 <aab_file>"
    echo "Example: $0 beggar_life0_27_01.aab"
    exit 1
fi

AAB_FILE="$1"
BASE_NAME="${AAB_FILE%.aab}"
APKS_FILE="${BASE_NAME}.apks"
OUTPUT_DIR="${BASE_NAME}_extracted"
FINAL_APK="${BASE_NAME}.apk"

# Check if AAB file exists
if [ ! -f "$AAB_FILE" ]; then
    echo "Error: AAB file '$AAB_FILE' not found"
    exit 1
fi

echo "Converting $AAB_FILE to APK..."

# Generate universal APK using bundletool
"jdk11.0.28_6/bin/java" -jar bundletool_all.jar build-apks \
    --bundle="$AAB_FILE" \
    --output="$APKS_FILE" \
    --mode=universal

if [ $? -ne 0 ]; then
    echo "Error: Failed to generate APKs"
    exit 1
fi

# Extract the APK from APKS file
unzip -o "$APKS_FILE" -d "$OUTPUT_DIR"

if [ $? -ne 0 ]; then
    echo "Error: Failed to extract APKs"
    exit 1
fi

# Copy universal APK to final location
cp "$OUTPUT_DIR/universal.apk" "$FINAL_APK"

if [ $? -ne 0 ]; then
    echo "Error: Failed to copy APK"
    exit 1
fi

# Clean up
rm -rf "$OUTPUT_DIR"
rm -f "$APKS_FILE"

echo "Success! APK created: $FINAL_APK"