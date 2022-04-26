import json

from django.http import HttpResponse
from django.http.response import HttpResponseBadRequest
from django.views.decorators.csrf import csrf_exempt
from django.shortcuts import render


@csrf_exempt
def index(request):
    print(request)
    dict = {
        "loginFeedback": True,
    }
    if request.method == 'POST':
        return HttpResponse(json.dumps(dict))

    return HttpResponse(json.dumps(dict))


# Create your views here.
