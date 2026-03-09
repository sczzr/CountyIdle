shader_type canvas_item;
render_mode unshaded;

uniform float distortion_strength : hint_range(0.0, 6.0) = 2.2;
uniform float diffusion_strength : hint_range(0.0, 1.0) = 0.28;
uniform float tint_strength : hint_range(0.0, 1.0) = 0.18;
uniform float saturation_strength : hint_range(0.0, 1.0) = 0.82;
uniform float contrast_strength : hint_range(0.5, 1.5) = 0.96;
uniform float grain_strength : hint_range(0.0, 0.08) = 0.02;
uniform vec4 glass_tint : source_color = vec4(0.95, 0.94, 0.90, 1.0);

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	float a = hash(i);
	float b = hash(i + vec2(1.0, 0.0));
	float c = hash(i + vec2(0.0, 1.0));
	float d = hash(i + vec2(1.0, 1.0));
	vec2 u = f * f * (3.0 - 2.0 * f);
	return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

void fragment() {
	vec2 cell = UV * vec2(180.0, 96.0);
	vec2 jitter = vec2(
		noise(cell + vec2(11.3, 17.1)) - 0.5,
		noise(cell + vec2(43.7, 29.5)) - 0.5
	);

	vec2 offset = jitter * TEXTURE_PIXEL_SIZE * distortion_strength * 12.0;
	vec4 base = texture(TEXTURE, UV + offset);
	vec4 secondary = texture(
		TEXTURE,
		UV + offset * 0.5 + TEXTURE_PIXEL_SIZE * vec2(2.0, -1.0) * distortion_strength
	);

	vec3 color = mix(base.rgb, secondary.rgb, diffusion_strength);
	float luma = dot(color, vec3(0.299, 0.587, 0.114));
	color = mix(vec3(luma), color, saturation_strength);
	color = (color - 0.5) * contrast_strength + 0.5;
	color = mix(color, glass_tint.rgb, tint_strength);

	float grain = (noise(UV * vec2(640.0, 360.0) + vec2(5.2, 9.4)) - 0.5) * grain_strength;
	color += vec3(grain);

	COLOR = vec4(clamp(color, vec3(0.0), vec3(1.0)), base.a);
}
