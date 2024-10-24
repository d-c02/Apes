shader_type spatial;
render_mode blend_mix, depth_draw_opaque, diffuse_burley, specular_schlick_ggx, world_vertex_coords, cull_back;
varying vec3 world_vertex;
varying vec3 world_normal;

uniform vec4 albedo : source_color;
uniform sampler2D texture_albedo : source_color, filter_linear_mipmap, repeat_enable;
uniform float point_size : hint_range(0.1, 128.0, 0.1);

uniform float roughness : hint_range(0.0, 1.0);
uniform sampler2D texture_metallic : hint_default_white, filter_linear_mipmap, repeat_enable;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_roughness_g, filter_linear_mipmap, repeat_enable;
uniform sampler2D texture_height : filter_linear_mipmap, repeat_enable;

uniform float specular : hint_range(0.0, 1.0, 0.01);
uniform float metallic : hint_range(0.0, 1.0, 0.01);

uniform sampler2D texture_normal : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform float normal_scale : hint_range(-16.0, 16.0);

uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;

void vertex() {
  world_vertex = VERTEX;
  world_normal = NORMAL;

  float height = texture(texture_height, world_vertex.xz / 20.0f).g;
  VERTEX.y += height * 0.3f;
}

void fragment() {
	vec2 base_uv = UV;
	//vec4 albedo_tex = texture(texture_albedo, base_uv);
	vec4 albedo_tex = texture(texture_albedo, world_vertex.xz / 1.0f);
	ALBEDO = albedo.rgb * albedo_tex.rgb; 
	
	float metallic_tex = dot(texture(texture_metallic, base_uv), metallic_texture_channel);
	METALLIC = metallic_tex * metallic;
	SPECULAR = specular;

	vec4 roughness_texture_channel = vec4(0.0, 1.0, 0.0, 0.0);
	float roughness_tex = dot(texture(texture_roughness, world_vertex.xz / 20.0f), roughness_texture_channel);
	ROUGHNESS = roughness_tex * roughness;

	// Normal Map: Enabled
	NORMAL_MAP = texture(texture_normal, world_vertex.xz / 20.0f).rgb;
	NORMAL_MAP_DEPTH = normal_scale;
}
