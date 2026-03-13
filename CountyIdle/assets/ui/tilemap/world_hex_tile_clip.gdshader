shader_type canvas_item;

uniform float hex_half_width : hint_range(0.6, 1.0) = 0.8660254;
uniform float edge_feather : hint_range(0.0, 0.08) = 0.01;

void fragment() {
    vec4 color = texture(TEXTURE, UV) * COLOR;

    vec2 p = UV * 2.0 - vec2(1.0);
    float ax = abs(p.x);
    float ay = abs(p.y);
    float edge = ay - (1.0 - (ax * 0.57735026919));
    edge = max(edge, ax - hex_half_width);
    float alpha = 1.0 - smoothstep(-edge_feather, edge_feather, edge);

    COLOR = vec4(color.rgb, color.a * alpha);
}
