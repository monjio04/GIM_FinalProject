from pathlib import Path
import argparse

from PIL import Image


SCRIPT_DIR = Path(__file__).resolve().parent


def merge_opacity_to_alpha(base_color_path, opacity_path, output_path, invert_opacity=False):
    base_color_path = Path(base_color_path)
    opacity_path = Path(opacity_path)
    output_path = Path(output_path)

    base_color = Image.open(base_color_path).convert("RGB")
    opacity = Image.open(opacity_path).convert("L")

    if opacity.size != base_color.size:
        opacity = opacity.resize(base_color.size, Image.Resampling.LANCZOS)

    if invert_opacity:
        opacity = Image.eval(opacity, lambda value: 255 - value)

    result = Image.merge("RGBA", (*base_color.split(), opacity))
    result.save(output_path)
    return output_path


def parse_args():
    parser = argparse.ArgumentParser(
        description="Merge a base color PNG with an opacity PNG as the alpha channel."
    )
    parser.add_argument(
        "--base",
        default=SCRIPT_DIR / "Conditioner_Base_color.png",
        help="Base color PNG path. Default: Conditioner_Base_color.png next to this script.",
    )
    parser.add_argument(
        "--opacity",
        default=SCRIPT_DIR / "Conditioner_Opacity.png",
        help="Opacity PNG path. White is opaque, black is transparent.",
    )
    parser.add_argument(
        "--output",
        default=SCRIPT_DIR / "Conditioner_Base_color_WithAlpha.png",
        help="Output RGBA PNG path.",
    )
    parser.add_argument(
        "--invert",
        action="store_true",
        help="Invert opacity before writing alpha. Use this when black means opaque.",
    )
    return parser.parse_args()


def main():
    args = parse_args()
    output_path = merge_opacity_to_alpha(
        base_color_path=args.base,
        opacity_path=args.opacity,
        output_path=args.output,
        invert_opacity=args.invert,
    )
    print(f"Saved: {output_path}")


if __name__ == "__main__":
    main()
