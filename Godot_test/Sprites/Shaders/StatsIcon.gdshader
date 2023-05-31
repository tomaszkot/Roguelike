shader_type canvas_item;

uniform vec3 custom_color : source_color = vec3(0.73, 0.73, 0.7);

void fragment() {
    // Get the color of the current pixel
    vec4 color = texture(TEXTURE, UV);
	color.rgb = mix(color.rgb, custom_color.rgb, 1);
    COLOR = color;
}
