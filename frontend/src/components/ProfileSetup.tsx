import React, { useState } from 'react';
import { useSaveCallerUserProfile, useIsUsernameAvailable } from '../hooks/useQueries';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { User, Sparkles, AlertCircle } from 'lucide-react';

export default function ProfileSetup() {
  const [name, setName] = useState('');
  const [validationError, setValidationError] = useState('');
  const [isValidating, setIsValidating] = useState(false);
  
  const saveProfile = useSaveCallerUserProfile();
  const checkUsername = useIsUsernameAvailable();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!name.trim()) {
      setValidationError('Please enter a username.');
      return;
    }

    setIsValidating(true);
    setValidationError('');

    try {
      // Check username availability on form submission
      const available = await checkUsername.mutateAsync(name.trim());
      
      if (!available) {
        setValidationError('This username is already taken. Please choose a different one.');
        setIsValidating(false);
        return;
      }

      // If username is available, proceed with profile creation
      await saveProfile.mutateAsync({ name: name.trim() });
    } catch (error: any) {
      if (error.message.includes('Username already exists')) {
        setValidationError('This username is already taken. Please choose a different one.');
      } else {
        setValidationError('Error checking username availability. Please try again.');
      }
      setIsValidating(false);
    }
  };

  const isSubmitting = isValidating || saveProfile.isPending;

  return (
    <div className="max-w-md mx-auto">
      <Card className="bg-white/10 backdrop-blur-sm border-white/20">
        <CardHeader className="text-center">
          <div className="flex items-center justify-center mb-4">
            <div className="bg-gradient-to-r from-purple-600 to-blue-600 p-3 rounded-full">
              <User className="h-8 w-8 text-white" />
            </div>
          </div>
          <CardTitle className="text-2xl text-white flex items-center justify-center gap-2">
            Welcome, Adventurer! <Sparkles className="h-5 w-5 text-purple-400" />
          </CardTitle>
          <CardDescription className="text-purple-200">
            Let's set up your profile to get started with your campaigns. Choose a unique username that others can use to invite you to their campaigns.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name" className="text-white">Your Username</Label>
              <Input
                id="name"
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter a unique username"
                required
                className="bg-white/10 border-white/20 text-white placeholder:text-purple-300"
                disabled={isSubmitting}
              />
              
              {validationError && (
                <Alert className="bg-red-500/10 border-red-500/20">
                  <AlertCircle className="h-4 w-4 text-red-400" />
                  <AlertDescription className="text-red-300">
                    {validationError}
                  </AlertDescription>
                </Alert>
              )}
            </div>
            
            <Button
              type="submit"
              disabled={!name.trim() || isSubmitting}
              className="w-full bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white border-0 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isSubmitting ? 'Creating Profile...' : 'Create Profile'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
