/**
 * Converts an RGB color value to HSL. Conversion formula
 * adapted from http://en.wikipedia.org/wiki/HSL_color_space.
 * Assumes r, g, and b are contained in the set [0, 255] and
 * returns h, s, and l in the set [0, 1].
 */
export function rgbToHsl(r, g, b) {
  r /= 255, g /= 255, b /= 255;
  const max = Math.max(r, g, b), min = Math.min(r, g, b);
  let h, s, l = (max + min) / 2;

  if (max === min) {
    h = s = 0; // achromatic
  } else {
    const d = max - min;
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
    switch (max) {
      case r: h = (g - b) / d + (g < b ? 6 : 0); break;
      case g: h = (b - r) / d + 2; break;
      case b: h = (r - g) / d + 4; break;
    }
    h /= 6;
  }

  return [h, s, l];
}

/**
 * Converts an HSL color value to RGB. Conversion formula
 * adapted from http://en.wikipedia.org/wiki/HSL_color_space.
 * Assumes h, s, and l are contained in the set [0, 1] and
 * returns r, g, and b in the set [0, 255].
 */
export function hslToRgb(h, s, l) {
  let r, g, b;

  if (s === 0) {
    r = g = b = l; // achromatic
  } else {
    const hue2rgb = (p, q, t) => {
      if (t < 0) t += 1;
      if (t > 1) t -= 1;
      if (t < 1 / 6) return p + (q - p) * 6 * t;
      if (t < 1 / 2) return q;
      if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
      return p;
    };

    const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
    const p = 2 * l - q;
    r = hue2rgb(p, q, h + 1 / 3);
    g = hue2rgb(p, q, h);
    b = hue2rgb(p, q, h - 1 / 3);
  }

  return [Math.round(r * 255), Math.round(g * 255), Math.round(b * 255)];
}

export function hexToRgb(hex) {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  return result ? [
    parseInt(result[1], 16),
    parseInt(result[2], 16),
    parseInt(result[3], 16)
  ] : null;
}

export function rgbToHex(r, g, b) {
  return "#" + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
}

export function getComplementary(hex) {
  const rgb = hexToRgb(hex);
  if (!rgb) return hex;
  const hsl = rgbToHsl(rgb[0], rgb[1], rgb[2]);
  hsl[0] = (hsl[0] + 0.5) % 1;
  const newRgb = hslToRgb(hsl[0], hsl[1], hsl[2]);
  return rgbToHex(newRgb[0], newRgb[1], newRgb[2]);
}

export function getAnalogous(hex, shift = 0.083) { // ~30 degrees
  const rgb = hexToRgb(hex);
  if (!rgb) return [hex, hex];
  const hsl = rgbToHsl(rgb[0], rgb[1], rgb[2]);
  
  const h1 = (hsl[0] + shift + 1) % 1;
  const h2 = (hsl[0] - shift + 1) % 1;
  
  const rgb1 = hslToRgb(h1, hsl[1], hsl[2]);
  const rgb2 = hslToRgb(h2, hsl[1], hsl[2]);
  
  return [rgbToHex(rgb1[0], rgb1[1], rgb1[2]), rgbToHex(rgb2[0], rgb2[1], rgb2[2])];
}

export function getTriadic(hex) {
  const rgb = hexToRgb(hex);
  if (!rgb) return [hex, hex];
  const hsl = rgbToHsl(rgb[0], rgb[1], rgb[2]);
  
  const h1 = (hsl[0] + 0.333) % 1;
  const h2 = (hsl[0] + 0.666) % 1;
  
  const rgb1 = hslToRgb(h1, hsl[1], hsl[2]);
  const rgb2 = hslToRgb(h2, hsl[1], hsl[2]);
  
  return [rgbToHex(rgb1[0], rgb1[1], rgb1[2]), rgbToHex(rgb2[0], rgb2[1], rgb2[2])];
}
