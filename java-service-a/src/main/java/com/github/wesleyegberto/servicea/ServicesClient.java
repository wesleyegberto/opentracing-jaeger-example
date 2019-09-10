package com.github.wesleyegberto.servicea;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import java.util.List;

@Service
public class ServicesClient {
	@Autowired
	private RestTemplate restTemplate;

	public List<String> getLanguages() {
		return restTemplate.getForObject("http://localhost:5000/serviceb/api/languages", List.class);
	}

	public List<String> getFrameworks() {
		return restTemplate.getForObject("http://localhost:8090/servicec/api/frameworks", List.class);
	}
}
