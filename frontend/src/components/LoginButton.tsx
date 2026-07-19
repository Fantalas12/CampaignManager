import React from 'react';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import { useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { LogIn, LogOut, Loader2 } from 'lucide-react';

export default function LoginButton() {
  const { login, clear, loginStatus, identity } = useInternetIdentity();
  const queryClient = useQueryClient();

  const isAuthenticated = !!identity;
  const disabled = loginStatus === 'logging-in';

  const handleAuth = async () => {
    if (isAuthenticated) {
      await clear();
      queryClient.clear();
    } else {
      try {
        await login();
      } catch (error: any) {
        console.error('Login error:', error);
        if (error.message === 'User is already authenticated') {
          await clear();
          setTimeout(() => login(), 300);
        }
      }
    }
  };

  return (
    <Button
      onClick={handleAuth}
      disabled={disabled}
      variant={isAuthenticated ? "outline" : "default"}
      className={`transition-all duration-200 ${
        isAuthenticated
          ? 'border-white/20 text-white hover:bg-white/10'
          : 'bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0'
      }`}
    >
      {disabled ? (
        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
      ) : isAuthenticated ? (
        <LogOut className="h-4 w-4 mr-2" />
      ) : (
        <LogIn className="h-4 w-4 mr-2" />
      )}
      {disabled ? 'Logging in...' : isAuthenticated ? 'Logout' : 'Login'}
    </Button>
  );
}
