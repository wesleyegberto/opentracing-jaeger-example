package com.github.wesleyegberto.servicea;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
@RequestMapping("/api")
public class ValuesController {
	@Autowired
	private ServicesClient service;

	@GetMapping("languages")
	public List<String> getLanguages() {
		return service.getLanguages();
	}

	@GetMapping("frameworks")
	public List<String> getFrameworks() {
		return service.getFrameworks();
	}
}
