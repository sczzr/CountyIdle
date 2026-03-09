shader_type canvas_item;
render_mode unshaded;

uniform float blur_radius : hint_range(0.0, 6.0) = 2.8;

void fragment() {
	vec2 texel = TEXTURE_PIXEL_SIZE * blur_radius;

	vec4 blurred =
		texture(TEXTURE, UV + texel * vec2(-1.0, -1.0)) * 1.0 +
		texture(TEXTURE, UV + texel * vec2( 0.0, -1.0)) * 2.0 +
		texture(TEXTURE, UV + texel * vec2( 1.0, -1.0)) * 1.0 +
		texture(TEXTURE, UV + texel * vec2(-1.0,  0.0)) * 2.0 +
		texture(TEXTURE, UV) * 4.0 +
		texture(TEXTURE, UV + texel * vec2( 1.0,  0.0)) * 2.0 +
		texture(TEXTURE, UV + texel * vec2(-1.0,  1.0)) * 1.0 +
		texture(TEXTURE, UV + texel * vec2( 0.0,  1.0)) * 2.0 +
		texture(TEXTURE, UV + texel * vec2( 1.0,  1.0)) * 1.0;

	COLOR = blurred / 16.0;
}
